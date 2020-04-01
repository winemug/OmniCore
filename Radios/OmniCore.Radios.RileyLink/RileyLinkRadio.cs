using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Newtonsoft.Json;
using Nito.AsyncEx;
using OmniCore.Model.Entities;
using OmniCore.Model.Enumerations;
using OmniCore.Model.Exceptions;
using OmniCore.Model.Interfaces.Common;
using OmniCore.Model.Interfaces.Services;
using OmniCore.Model.Interfaces.Services.Facade;
using OmniCore.Model.Interfaces.Services.Internal;
using OmniCore.Model.Utilities;
using OmniCore.Model.Utilities.Extensions;

namespace OmniCore.Radios.RileyLink
{
    public class RileyLinkRadio : IErosRadio
    {
        public IBlePeripheral Peripheral { get; set; }
        public RadioEntity Entity { get; set; }
        public async Task<ILease<IErosRadio>> Lease(CancellationToken cancellationToken)
        {
            return await Lease<IErosRadio>.NewLease(this, cancellationToken);
        }

        public bool OnLease { get; set; }
        public void ThrowIfNotOnLease()
        {
            if (!OnLease)
                throw new OmniCoreWorkflowException(FailureType.Internal, "object needs to be on lease for this operation");
        }

        public RadioOptions Options
        {
            get
            {
                if (ActiveOptions != null)
                    return ActiveOptions;
                return Entity.Options;
            }
        }

        public RadioType Type => RadioType.RileyLink;

        public string Address => Peripheral.PeripheralUuid.AsMacAddress();
        public string Description => Entity.UserDescription;
        public IObservable<string> Name => Peripheral.WhenNameUpdated();
        public IObservable<PeripheralDiscoveryState> DiscoveryState => Peripheral.WhenDiscoveryStateChanged();
        public IObservable<PeripheralConnectionState> ConnectionState => Peripheral.WhenConnectionStateChanged();
        public IObservable<int> Rssi => Peripheral.WhenRssiReceived();
        public async Task SetDescription(string description, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public bool InUse { get; set; }
        private RadioActivity ActivityInternal = RadioActivity.Idle;
        public RadioActivity Activity
        {
            get => ActivityInternal;
            set
            {
                ActivityInternal = value;
                ActivityStartDate = DateTimeOffset.UtcNow;
            }
        }
        public DateTimeOffset? ActivityStartDate { get; private set; } = DateTimeOffset.UtcNow;

        private IDisposable ConnectedSubscription = null;
        private IDisposable ConnectionFailedSubscription = null;
        private IDisposable DisconnectedSubscription = null;
        private IDisposable PeripheralStateSubscription = null;
        private IDisposable PeripheralRssiSubscription = null;
        private IDisposable ResponseNotifySubscription = null;
        private IDisposable PeripheralLocateSubscription = null;

        private readonly Guid RileyLinkServiceUuid = Guid.Parse("0235733b-99c5-4197-b856-69219c2a3845");
        private readonly Guid RileyLinkDataCharacteristicUuid = Guid.Parse("c842e849-5028-42e2-867c-016adada9155");
        private readonly Guid RileyLinkResponseCharacteristicUuid = Guid.Parse("6e6c7910-b89e-43a5-a0fe-50c5e2b81f4a");

        private ConcurrentQueue<byte[]> Responses;
        private readonly AsyncManualResetEvent ResponseEvent;

        private RadioOptions ActiveOptions = null;

        private readonly ICoreContainer<IServerResolvable> Container;
        private readonly ICoreLoggingFunctions Logging;
        private readonly ICoreRepositoryService RepositoryService;

        public RileyLinkRadio(
            ICoreContainer<IServerResolvable> container,
            ICoreLoggingFunctions logging,
            ICoreRepositoryService repositoryService)
        {
            RepositoryService = repositoryService;
            Container = container;
            Logging = logging;
            Responses = new ConcurrentQueue<byte[]>();
            ResponseEvent = new AsyncManualResetEvent();
        }
        private async Task RecordRadioEvent(RadioEvent eventType, CancellationToken cancellationToken,
            string text = null, byte[] data = null, int? rssi = null)
        {
            Logging.Debug($"RLR: {Address} Recording radio event {eventType}");

            using var context = await RepositoryService.GetWriterContext(cancellationToken);
            await context.RadioEvents.AddAsync(
                new RadioEventEntity
                {
                    Radio = Entity,
                    EventType = eventType,
                    Text = text,
                    Data = data,
                    Rssi = rssi
                }, cancellationToken);
            await context.Save(cancellationToken);
        }

        public async Task Identify(CancellationToken cancellationToken)
        {
            ThrowIfNotOnLease();

            Logging.Debug($"RLR: {Address} Identifying device");
            using var connection = await Connect(cancellationToken);
            
            await SendCommandsAndGetResponses(connection, cancellationToken, TimeSpan.FromSeconds(10),
                new List<(RileyLinkCommandType, byte[])>
            {
                (RileyLinkCommandType.Led, new byte[] {0, 0}),
                (RileyLinkCommandType.Led, new byte[] {1, 1}),
                (RileyLinkCommandType.GetState, null),
                (RileyLinkCommandType.Led, new byte[] {0, 1}),
                (RileyLinkCommandType.Led, new byte[] {1, 0}),
                (RileyLinkCommandType.GetState, null),
                (RileyLinkCommandType.Led, new byte[] {0, 0}),
                (RileyLinkCommandType.Led, new byte[] {1, 1}),
                (RileyLinkCommandType.GetState, null),
                (RileyLinkCommandType.Led, new byte[] {0, 1}),
                (RileyLinkCommandType.Led, new byte[] {1, 0}),
                (RileyLinkCommandType.GetState, null),
                (RileyLinkCommandType.Led, new byte[] {0, 0}),
                (RileyLinkCommandType.Led, new byte[] {1, 1}),
                (RileyLinkCommandType.GetState, null),
                (RileyLinkCommandType.GetState, null),
                (RileyLinkCommandType.Led, new byte[] {1, 0}),
            });
        }

        public void StartMonitoring()
        {
            Logging.Debug($"RLR: {Address} Start monitoring");

            PeripheralRssiSubscription?.Dispose();

            if (Entity.Options.RssiUpdateInterval.HasValue)
            {
                PeripheralRssiSubscription = Observable.Interval(Entity.Options.RssiUpdateInterval.Value)
                    .Subscribe( async _ =>
                    {
                        try
                        {
                            Logging.Debug($"RLR: {Address} Rssi request");
                            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
                            var rssi = await Peripheral.ReadRssi(cts.Token);
                            Logging.Debug($"RLR: {Address} Rssi received: {rssi}");
                            await RecordRadioEvent(RadioEvent.RssiReceived, CancellationToken.None, null,
                                null, rssi);
                        }
                        catch (OperationCanceledException)
                        {
                            Logging.Debug($"RLR: {Address} Rssi timed out!");
                        }
                    });
            }

            ConnectedSubscription?.Dispose();
            ConnectedSubscription = Peripheral.WhenConnectionStateChanged()
                .FirstAsync(s => s == PeripheralConnectionState.Connected)
                .Subscribe( async (_) =>
                {
                    Logging.Debug($"RLR: {Address} Connected");
                    ActiveOptions = null;
                    await RecordRadioEvent(RadioEvent.Connect, CancellationToken.None);
                });

            DisconnectedSubscription?.Dispose();
            DisconnectedSubscription = Peripheral.WhenConnectionStateChanged()
                .FirstAsync(s => s == PeripheralConnectionState.Disconnected)
                .Subscribe( async (_) =>
                {
                    Logging.Debug($"RLR: {Address} Disconnected");
                    ResponseNotifySubscription?.Dispose();
                    ResponseNotifySubscription = null;
                    ActiveOptions = null;

                    await RecordRadioEvent(RadioEvent.Disconnect, CancellationToken.None);
                });

            PeripheralLocateSubscription?.Dispose();
            PeripheralLocateSubscription = null;
            
            PeripheralStateSubscription?.Dispose();
            PeripheralStateSubscription = Peripheral.WhenDiscoveryStateChanged()
                .Subscribe(async (state) =>
            {
                switch (state)
                {
                    case PeripheralDiscoveryState.Unknown:
                        Logging.Debug($"RLR: {Address} Peripheral state unknown");
                        var cts = new CancellationTokenSource(Entity.Options.RadioDiscoveryTimeout);
                        await Peripheral.Discover(cts.Token);
                        break;
                    case PeripheralDiscoveryState.NotFound:
                        Logging.Debug($"RLR: {Address} Peripheral not found in last scan");
                        await RecordRadioEvent(RadioEvent.Offline, CancellationToken.None);
                        PeripheralLocateSubscription = Observable.Interval(Entity.Options.RadioDiscoveryCooldown)
                            .Subscribe(async _ =>
                            {
                                var cts = new CancellationTokenSource(Entity.Options.RadioDiscoveryTimeout);
                                await Peripheral.Discover(cts.Token);
                            });
                        break;
                    case PeripheralDiscoveryState.Searching:
                        Logging.Debug($"RLR: {Address} Looking for peripheral");
                        break;
                    case PeripheralDiscoveryState.Discovered:
                        if (PeripheralLocateSubscription != null)
                        {
                            PeripheralLocateSubscription.Dispose();
                            PeripheralLocateSubscription = null;
                        }
                        Logging.Debug($"RLR: {Address} Peripheral is online");
                        await RecordRadioEvent(RadioEvent.Online, CancellationToken.None);
                        
                        if (Entity.Options.KeepConnected)
                        {
                            Logging.Debug($"RLR: {Address} Connecting to peripheral due to KeepConnected setting");
                            try
                            {
                                using var connection = await Connect(CancellationToken.None);
                            }
                            catch (OperationCanceledException e)
                            {
                            }
                        }
                        break;
                }
            });
        }

        private async Task Initialize(IBlePeripheralConnection connection,
            CancellationToken cancellationToken, RadioOptions options)
        {

            if (options == null)
                options = Entity.Options;

            if (ActiveOptions != null)
            {
                if (GetParameters(ActiveOptions).SequenceEqual(GetParameters(options)))
                    return;
            }
            await ConfigureRileyLink(connection, cancellationToken, options);            
        }
        
        private async Task<IBlePeripheralConnection> Connect(CancellationToken cancellationToken)
        {
            using var connectionTimeoutOverall = new CancellationTokenSource(Entity.Options.RadioConnectionOverallTimeout);
            using var linkedCancellation = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken,
                connectionTimeoutOverall.Token);
            try
            {
                Logging.Debug($"RLR: {Address} Connecting..");
                var connection = await Peripheral.GetConnection(Options.AutoConnect, Options.KeepConnected,
                    Options.RadioDiscoveryTimeout, Options.RadioConnectTimeout,
                    Options.RadioCharacteristicsDiscoveryTimeout,
                    linkedCancellation.Token);
                
                connection
                    .WhenCharacteristicNotificationReceived(RileyLinkServiceUuid, RileyLinkResponseCharacteristicUuid)
                    .Subscribe(async (_) =>
                    {
                        Logging.Debug($"RLR: {Address} Characteristic notification received");
                        while (true)
                        {
                            using var notifyReadTimeout =
                                new CancellationTokenSource(Entity.Options.RadioResponseTimeout);
                            var commandResponse = await connection.ReadFromCharacteristic(RileyLinkServiceUuid, RileyLinkDataCharacteristicUuid,
                                notifyReadTimeout.Token);

                            if (commandResponse != null && commandResponse.Length > 0)
                            {
                                Logging.Debug($"RLR: {Address} Characteristic notification read: {BitConverter.ToString(commandResponse)}");
                                Responses.Enqueue(commandResponse);
                                ResponseEvent.Set();
                                break;
                            }
                        }
                    });

                Logging.Debug($"RLR: {Address} Requesting rssi..");
                await Peripheral.ReadRssi(cancellationToken);

                return connection;
            }
            catch (Exception e)
            {
                Logging.Debug($"RLR: {Address} Error while connecting:\n {e.AsDebugFriendly()}");
                ResponseNotifySubscription?.Dispose();
                ResponseNotifySubscription = null;
                ActiveOptions = null;

                await RecordRadioEvent(RadioEvent.Error, CancellationToken.None,
                    $"Connect failed: {e.AsDebugFriendly()}");

                throw new OmniCoreRadioException(FailureType.RadioGeneralError);
                
            }
        }

        private async Task ConfigureRileyLink(IBlePeripheralConnection connection, CancellationToken cancellationToken, RadioOptions options)
        {
            Activity = RadioActivity.Configuring;
            Logging.Debug($"RLR: {Address} Configuring rileylink");
            await SendCommandWithoutResponse(connection, cancellationToken, RileyLinkCommandType.Reset);
            await SendCommandGetResponse(connection, cancellationToken, TimeSpan.FromSeconds(1), RileyLinkCommandType.None);
            //await SendCommandWithoutResponse(cancellationToken, RileyLinkCommandType.ResetRadioConfig);

            var commands = new List<(RileyLinkCommandType Command, byte[] Data)>();

            commands.Add((RileyLinkCommandType.SetSwEncoding, new byte[] { (byte)RileyLinkSoftwareEncoding.None }));
            commands.Add((RileyLinkCommandType.SetPreamble, new byte[] { 0x55, 0x55 }));

            foreach (var register in GetParameters(options))
                commands.Add((RileyLinkCommandType.UpdateRegister, new[] { (byte)register.Item1, (byte)register.Item2 })) ;

            var responses = await SendCommandsAndGetResponses(connection, cancellationToken, TimeSpan.FromSeconds(15), commands);
            if (responses.Any(r => r.Type != RileyLinkResponseType.OK))
                throw new OmniCoreRadioException(FailureType.RadioErrorResponse, "Failed to configure RileyLink");

            var (resultType, resultData) = await SendCommandGetResponse(connection, cancellationToken, TimeSpan.FromSeconds(2), RileyLinkCommandType.GetState);
            if (resultType != RileyLinkResponseType.OK || resultData.Length != 2 || resultData[0] != 'O' || resultData[1] != 'K')
                throw new OmniCoreRadioException(FailureType.RadioErrorResponse, "RL status is not 'OK'");
            
            Activity = RadioActivity.Idle;
            ActiveOptions = options;
        }

        private async Task<List<(RileyLinkResponseType Type, byte[] Data)>> SendCommandsAndGetResponses(
            IBlePeripheralConnection connection,
            CancellationToken cancellationToken,
            TimeSpan expectedProcessingDuration, List<(RileyLinkCommandType Command, byte[] Data)> commandList)
        {
            Logging.Debug($"RLR: {Address} Sending batch commands");
            var resultList = new List<(RileyLinkResponseType Type, byte[] Data)>();
            var data = new Bytes((byte) 0);
            foreach (var command in commandList)
            {
                data.Append((byte)command.Command);
                if (command.Item2 != null)
                    data.Append(command.Data);
            }

            var dataArray = data.ToArray();
            dataArray[0] = (byte) (dataArray.Length - 1);

            using var responseTimeout = new CancellationTokenSource
                (Entity.Options.RadioResponseTimeout * commandList.Count);
            
            using var linkedCancellation =
                CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, responseTimeout.Token);
            
            //Debug.WriteLine($"{DateTimeOffset.Now} RL: Writing {BitConverter.ToString(dataArray)} Count: {commandList.Count}");
            await connection.WriteToCharacteristic(
                RileyLinkServiceUuid, RileyLinkDataCharacteristicUuid,
                dataArray, linkedCancellation.Token);
            //Debug.WriteLine($"{DateTimeOffset.Now} RL: Written {BitConverter.ToString(dataArray)}");
            for (int i = 0; i < commandList.Count; i++)
            {
                resultList.Add(await GetResponse(cancellationToken, expectedProcessingDuration));
            }

            return resultList;
        }

        private async Task SendCommandWithoutResponse(
            IBlePeripheralConnection connection,
            CancellationToken cancellationToken,
            RileyLinkCommandType cmd, byte[] cmdData = null)
        {
            Logging.Debug($"RLR: {Address} Sending batch commands w/o response");

            var data = GetCommandData(cmd, cmdData);
            using var responseTimeout = new CancellationTokenSource(Entity.Options.RadioResponseTimeout);
            using var linkedCancellation =
                CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, responseTimeout.Token);
            //Debug.WriteLine($"{DateTimeOffset.Now} RL: Writing noresponse {BitConverter.ToString(data)}");
            await connection.WriteToCharacteristicWithoutResponse(
                RileyLinkServiceUuid, RileyLinkDataCharacteristicUuid,
                data, linkedCancellation.Token);
            //Debug.WriteLine($"{DateTimeOffset.Now} RL: Written noresponse {BitConverter.ToString(data)}");
        }

        private async Task<(RileyLinkResponseType Type, byte[] Data)>
            SendCommandGetResponse(
                IBlePeripheralConnection connection,
                CancellationToken cancellationToken,
                TimeSpan expectedProcessingDuration,
                RileyLinkCommandType cmd,
                byte[] cmdData = null)
        {
            Logging.Debug($"RLR: {Address} Send single command");

            var data = GetCommandData(cmd, cmdData);

            using var responseTimeout = new CancellationTokenSource(Entity.Options.RadioResponseTimeout);
            using var linkedCancellation =
                CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, responseTimeout.Token);
            //Debug.WriteLine($"{DateTimeOffset.Now} RL: Writing {BitConverter.ToString(data)}");
            await connection.WriteToCharacteristic(
                RileyLinkServiceUuid, RileyLinkDataCharacteristicUuid,
                data, linkedCancellation.Token);
            //Debug.WriteLine($"{DateTimeOffset.Now} RL: Written {BitConverter.ToString(data)}");

            var response = await GetResponse(cancellationToken, expectedProcessingDuration);

            return response;
        }

        private byte[] GetCommandData(RileyLinkCommandType cmdType, byte[] cmdData = null)
        {
            byte[] data;
            if (cmdData == null)
            {
                data = new byte[] { 1, (byte)cmdType };
            }
            else
            {
                data = new byte[cmdData.Length + 2];
                data[0] = (byte)(cmdData.Length + 1);
                data[1] = (byte) cmdType;
                Buffer.BlockCopy(cmdData, 0, data, 2, cmdData.Length);
            }
            return data;
        }

        private async Task<(RileyLinkResponseType Type, byte[] Data)>
            GetResponse(CancellationToken cancellationToken,
                TimeSpan expectedProcessingDuration)
        {
            Logging.Debug($"RLR: {Address} Waiting for RL response");

            byte[] result = null;
            while (true)
            {
                ResponseEvent.Reset();
                if (Responses.IsEmpty)
                {
                    using var responseTimeout = new CancellationTokenSource(Entity.Options.RadioResponseTimeout.Add(expectedProcessingDuration));
                    using var linkedCancellation =
                        CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, responseTimeout.Token);
                    
                    await ResponseEvent.WaitAsync(linkedCancellation.Token);
                }
                else
                {
                    if (Responses.TryDequeue(out result))
                        break;
                }
            }

            if (result == null || result.Length == 0)
                throw new OmniCoreRadioException(FailureType.RadioInvalidResponse, "Zero length response received");

            var responseType = (RileyLinkResponseType)result[0];
            var response = new byte[result.Length - 1];
            Buffer.BlockCopy(result, 1, response, 0, response.Length);
            return (responseType, response);
        }

        private List<Tuple<RileyLinkRegister, int>> GetParameters(RadioOptions configuration)
        {
            var registers = new List<Tuple<RileyLinkRegister, int>>();

            registers.Add(Tuple.Create(RileyLinkRegister.SYNC0, 0x5A));
            registers.Add(Tuple.Create(RileyLinkRegister.SYNC1, 0xA5));
            registers.Add(Tuple.Create(RileyLinkRegister.PKTLEN, 0x50));

            var frequency = (int)(433910000 / (24000000 / Math.Pow(2, 16)));
            frequency += configuration.FrequencyShift;
            registers.Add(Tuple.Create(RileyLinkRegister.FREQ0, frequency & 0xff));
            registers.Add(Tuple.Create(RileyLinkRegister.FREQ1, (frequency >> 8) & 0xff));
            registers.Add(Tuple.Create(RileyLinkRegister.FREQ2, (frequency >> 16) & 0xff));

            registers.Add(Tuple.Create(RileyLinkRegister.DEVIATN, 0x44));

            registers.Add(Tuple.Create(RileyLinkRegister.FREND0, 0x00));
            int amplification;
            switch (configuration.Amplification)
            {
                case TransmissionPower.Lowest:
                    amplification = 0x0E;
                    break;
                case TransmissionPower.VeryLow:
                    amplification = 0x1D;
                    break;
                case TransmissionPower.Low:
                    amplification = 0x34;
                    break;
                case TransmissionPower.BelowNormal:
                    amplification = 0x2C;
                    break;
                case TransmissionPower.Normal:
                    amplification = 0x60;
                    break;
                case TransmissionPower.High:
                    amplification = 0x84;
                    break;
                case TransmissionPower.VeryHigh:
                    amplification = 0xC8;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            registers.Add(Tuple.Create(RileyLinkRegister.PATABLE0, amplification));

            registers.Add(Tuple.Create(RileyLinkRegister.FSCTRL0, 0x00));
            registers.Add(Tuple.Create(RileyLinkRegister.FSCTRL1, configuration.RxIntermediateFrequency));

            var pktctrl1 = configuration.PqeThreshold << 5;
            pktctrl1 &= 0xE0;

            var pktctrl0 = configuration.DataWhitening ? 0x40 : 0x00;

            registers.Add(Tuple.Create(RileyLinkRegister.PKTCTRL1, pktctrl1));
            registers.Add(Tuple.Create(RileyLinkRegister.PKTCTRL0, pktctrl0));

            var mcfg4 = configuration.FilterBWExponent << 6;
            mcfg4 |= configuration.FilterBWDecimationRatio << 4;
            mcfg4 &= 0xF0;
            mcfg4 |= 0x0A;
            registers.Add(Tuple.Create(RileyLinkRegister.MDMCFG4, mcfg4));
            registers.Add(Tuple.Create(RileyLinkRegister.MDMCFG3, 0xBC));

            var mcfg2 = configuration.PreambleCheckWithCarrierSense ? 0x06 : 0x02;
            registers.Add(Tuple.Create(RileyLinkRegister.MDMCFG2, mcfg2));

            var mcfg1 = configuration.ForwardErrorCorrection ? 0x80 : 0x00;
            mcfg1 |= configuration.TxPreambleCountSetting << 4;
            registers.Add(Tuple.Create(RileyLinkRegister.MDMCFG1, mcfg1));
            registers.Add(Tuple.Create(RileyLinkRegister.MDMCFG0, 0xF8));

            var mcsm0 = 0x18 | configuration.RxAttenuationLevel;
            registers.Add(Tuple.Create(RileyLinkRegister.MCSM0, mcsm0));

            registers.Add(Tuple.Create(RileyLinkRegister.MCSM0, mcsm0));

            registers.Add(Tuple.Create(RileyLinkRegister.FOCCFG, 0x17));
            registers.Add(Tuple.Create(RileyLinkRegister.FSCAL3, 0xE9));
            registers.Add(Tuple.Create(RileyLinkRegister.FSCAL2, 0x2A));
            registers.Add(Tuple.Create(RileyLinkRegister.FSCAL1, 0x00));
            registers.Add(Tuple.Create(RileyLinkRegister.FSCAL0, 0x1F));
            registers.Add(Tuple.Create(RileyLinkRegister.TEST1, 0x35));
            registers.Add(Tuple.Create(RileyLinkRegister.TEST0, 0x09));

            return registers;
        }

        public async Task<byte[]> GetResponse(IErosPodRequest request, CancellationToken cancellationToken,
            RadioOptions options)
        {
            using var connection = await Connect(cancellationToken);
            await Initialize(connection, cancellationToken, options);

            Logging.Debug($"RLR: {Address} Starting conversation with pod id: {request.ErosPod.Entity.Id}");
            var conversation = RileyLinkErosConversation.ForPod(request.ErosPod);
            conversation.Initialize(request);

            while (!conversation.IsFinished)
            {
                var sendPacketData = conversation.GetPacketToSend();

                var arguments = new Bytes().Append((byte) 0)
                    .Append((byte) 0).Append((ushort) 0).Append((byte) 0)
                    .Append((uint) 1600).Append((byte) 0)
                    .Append((ushort) 120);

                var result = await SendCommandGetResponse(
                    connection,
                    cancellationToken,
                    TimeSpan.FromMilliseconds(2000),
                    RileyLinkCommandType.SendPacket,
                    arguments.ToArray());

                var rssi = GetRssi(result.Data[0]);
                conversation.ParseIncomingPacket(result.Data[1..]);
            }

            var response = conversation.ResponseData.ToArray();
            Logging.Debug($"RLR: {Address} Conversation ended.");
            return response;
        }

        // public async Task<(byte Rssi, byte[] Data)> DebugGetPacket(uint timeoutMilliseconds,
        //     CancellationToken cancellationToken)
        // {
        //     await Connect(cancellationToken);
        //     Activity = RadioActivity.Listening;
        //     var arguments = new Bytes((byte) 0).Append(timeoutMilliseconds);
        //     var result = await SendCommandGetResponse(cancellationToken, TimeSpan.FromMilliseconds(timeoutMilliseconds),
        //         RileyLinkCommandType.GetPacket,
        //         arguments.ToArray());
        //     Activity = RadioActivity.Idle;
        //     await Disconnect(cancellationToken);
        //     if (result.Type != RileyLinkResponseType.OK)
        //         throw new OmniCoreRadioException(FailureType.RadioErrorResponse, $"RL returned: {result.Type}");
        //
        //     return (result.Data[0], result.Data[1..]);
        // }

        public void Dispose()
        {
            PeripheralRssiSubscription?.Dispose();
            PeripheralRssiSubscription = null;

            PeripheralStateSubscription?.Dispose();
            PeripheralStateSubscription = null;

            ResponseNotifySubscription?.Dispose();
            ResponseNotifySubscription = null;

            ActiveOptions = null;
            InUse = false;

            ConnectedSubscription?.Dispose();
            ConnectedSubscription = null;

            ConnectionFailedSubscription?.Dispose();
            ConnectionFailedSubscription = null;

            DisconnectedSubscription?.Dispose();
            DisconnectedSubscription = null;
        }

        private int GetRssi(byte rssiByte)
        {
            int rssi = rssiByte;
            return (rssi - 255) >> 2;
        }
    }
}
