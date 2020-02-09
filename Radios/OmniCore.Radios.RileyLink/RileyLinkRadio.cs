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
using Newtonsoft.Json;
using Nito.AsyncEx;
using OmniCore.Model.Constants;
using OmniCore.Model.Entities;
using OmniCore.Model.Enumerations;
using OmniCore.Model.Exceptions;
using OmniCore.Model.Extensions;
using OmniCore.Model.Interfaces.Data;
using OmniCore.Model.Interfaces.Platform.Common;
using OmniCore.Model.Interfaces.Platform.Server;
using OmniCore.Model.Utilities;

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

        public RadioType Type => RadioType.RileyLink;

        public string Address => Peripheral.PeripheralUuid.AsMacAddress();
        public string Description => Entity.UserDescription;
        public IObservable<string> Name => Peripheral.Name;
        public IObservable<PeripheralState> State => Peripheral.State;
        public IObservable<PeripheralConnectionState> ConnectionState => Peripheral.ConnectionState;
        public IObservable<int> Rssi => Peripheral.Rssi;
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
        private IDisposable LocateSubscription = null;
        private IDisposable ResponseNotifySubscription = null;

        private readonly Guid RileyLinkServiceUuid = Guid.Parse("0235733b-99c5-4197-b856-69219c2a3845");
        private readonly Guid RileyLinkDataCharacteristicUuid = Guid.Parse("c842e849-5028-42e2-867c-016adada9155");
        private readonly Guid RileyLinkResponseCharacteristicUuid = Guid.Parse("6e6c7910-b89e-43a5-a0fe-50c5e2b81f4a");

        private ConcurrentQueue<byte[]> Responses;
        private readonly AsyncManualResetEvent ResponseEvent;

        private RadioOptions ActiveOptions;
        private bool IsConfigured = false;

        private readonly ICoreContainer<IServerResolvable> Container;

        public RileyLinkRadio(
            ICoreContainer<IServerResolvable> container)
        {
            Container = container;
            Responses = new ConcurrentQueue<byte[]>();
            ResponseEvent = new AsyncManualResetEvent();
        }
        private async Task RecordRadioEvent(RadioEvent eventType, CancellationToken cancellationToken,
            string text = null, byte[] data = null, int? rssi = null)
        {
            var context = Container.Get<IRepositoryContext>();
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

            await Connect(cancellationToken);
            await SendCommandsAndGetResponses(cancellationToken, TimeSpan.FromSeconds(10),
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
            await Disconnect(cancellationToken);
        }

        public void StartMonitoring()
        {
            PeripheralRssiSubscription?.Dispose();
            PeripheralRssiSubscription = Peripheral.Rssi.Subscribe(async (rssi) =>
            {
                await RecordRadioEvent(RadioEvent.RssiReceived, CancellationToken.None, null,
                    null, rssi);
            });
            Peripheral.RssiAutoUpdateInterval = Entity.Options.RssiUpdateInterval;

            ConnectedSubscription?.Dispose();
            ConnectedSubscription = Peripheral.ConnectionState.Where(s => s == PeripheralConnectionState.Connected)
                .Subscribe( async (_) =>
                {
                    IsConfigured = false;
                    await RecordRadioEvent(RadioEvent.Connect, CancellationToken.None);
                });

            DisconnectedSubscription?.Dispose();
            DisconnectedSubscription = Peripheral.ConnectionState.Where(s => s == PeripheralConnectionState.Disconnected)
                .Subscribe( async (_) =>
                {
                    ResponseNotifySubscription?.Dispose();
                    ResponseNotifySubscription = null;
                    IsConfigured = false;

                    await RecordRadioEvent(RadioEvent.Disconnect, CancellationToken.None);
                });

            PeripheralStateSubscription?.Dispose();
            PeripheralStateSubscription = Peripheral.State.Subscribe(async (state) =>
            {
                LocateSubscription?.Dispose();
                switch (state)
                {
                    case PeripheralState.Unknown:
                        LocateSubscription = Peripheral
                            .Locate()
                            .Timeout(Entity.Options.RadioDiscoveryTimeout)
                            .Subscribe();
                        break;
                    case PeripheralState.Offline:
                        await RecordRadioEvent(RadioEvent.Offline, CancellationToken.None);
                        LocateSubscription = Observable.Timer(Entity.Options.RadioDiscoveryCooldown)
                            .Select(l => Peripheral)
                            .Concat(
                                Peripheral
                                    .Locate()
                                    .Timeout(Entity.Options.RadioDiscoveryTimeout)
                            ).Subscribe();
                            
                        break;
                    case PeripheralState.Online:
                        LocateSubscription = null;
                        await RecordRadioEvent(RadioEvent.Online, CancellationToken.None);
                        if (Entity.Options.KeepConnected)
                        {
                            using var cts = new CancellationTokenSource(Entity.Options.RadioConnectTimeout);
                            try
                            {
                                await Peripheral.Connect(true, cts.Token);
                            }
                            catch (OperationCanceledException e)
                            {
                            }
                        }
                        break;
                }
            });
        }

        public async Task Initialize(CancellationToken cancellationToken)
        {
            await Connect(cancellationToken);

            if (IsConfigured)
            {
                if (GetParameters(ActiveOptions).SequenceEqual(GetParameters(Entity.Options)))
                    return;
                else
                    ActiveOptions = Entity.Options;
            }
            await ConfigureRileyLink(cancellationToken);
        }

        public async Task ExecuteRequest(IPodRequest request, CancellationToken cancellationToken)
        {
            await Connect(cancellationToken);
            await Disconnect(cancellationToken);
        }

        public async Task<(byte Rssi, byte[] Data)> DebugGetPacket(uint timeoutMilliseconds,
            CancellationToken cancellationToken)
        {
            await Connect(cancellationToken);
            Activity = RadioActivity.Listening;
            var arguments = new Bytes((byte) 0).Append(timeoutMilliseconds);
            var result = await SendCommandGetResponse(cancellationToken, TimeSpan.FromMilliseconds(timeoutMilliseconds),
                RileyLinkCommandType.GetPacket,
                arguments.ToArray());
            Activity = RadioActivity.Idle;
            await Disconnect(cancellationToken);
            if (result.Type != RileyLinkResponseType.OK)
                throw new OmniCoreRadioException(FailureType.RadioErrorResponse, $"RL returned: {result.Type}");

            return (result.Data[0], result.Data[1..]);
        }

        public void Dispose()
        {
            Peripheral.RssiAutoUpdateInterval = null;
            PeripheralRssiSubscription?.Dispose();
            PeripheralRssiSubscription = null;

            PeripheralStateSubscription?.Dispose();
            PeripheralStateSubscription = null;

            ResponseNotifySubscription?.Dispose();
            ResponseNotifySubscription = null;

            IsConfigured = false;
            InUse = false;

            ConnectedSubscription?.Dispose();
            ConnectedSubscription = null;

            ConnectionFailedSubscription?.Dispose();
            ConnectionFailedSubscription = null;

            DisconnectedSubscription?.Dispose();
            DisconnectedSubscription = null;
        }

        private async Task Connect(CancellationToken cancellationToken)
        {
            using var _ = Peripheral.Lease(cancellationToken);

            using var connectionTimeout = new CancellationTokenSource(Entity.Options.RadioConnectTimeout);
            using (var linkedCancellation =
                CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, connectionTimeout.Token))
            {
                try
                {
                    await Peripheral.Connect(false, linkedCancellation.Token);
                }
                catch (Exception e)
                {
                    ResponseNotifySubscription?.Dispose();
                    ResponseNotifySubscription = null;
                    IsConfigured = false;

                    await RecordRadioEvent(RadioEvent.Error, CancellationToken.None,
                        $"Connect failed: {e.AsDebugFriendly()}");
                }
            }

            ResponseNotifySubscription = Peripheral
                .WhenCharacteristicNotificationReceived(RileyLinkServiceUuid, RileyLinkResponseCharacteristicUuid)
                .Subscribe(async (_) =>
                {
                    while (true)
                    {
                        using var notifyReadTimeout =
                            new CancellationTokenSource(Entity.Options.RadioResponseTimeout);
                        var commandResponse = await Peripheral.ReadFromCharacteristic(RileyLinkServiceUuid, RileyLinkDataCharacteristicUuid,
                            notifyReadTimeout.Token);

                        if (commandResponse != null && commandResponse.Length > 0)
                        {
                            //Debug.WriteLine($"{DateTimeOffset.Now} RL: Response {BitConverter.ToString(commandResponse)}");
                            Responses.Enqueue(commandResponse);
                            ResponseEvent.Set();
                            break;
                        }
                    }
                });
        }

        private async Task Disconnect(CancellationToken cancellationToken)
        {
            if (!Entity.Options.KeepConnected)
            {
                await Peripheral.Disconnect(cancellationToken);
            }
        }


        private async Task ConfigureRileyLink(CancellationToken cancellationToken)
        {
            Activity = RadioActivity.Configuring;
            await SendCommandWithoutResponse(cancellationToken, RileyLinkCommandType.Reset);
            await SendCommandGetResponse(cancellationToken, TimeSpan.FromSeconds(1), RileyLinkCommandType.None);
            //await SendCommandWithoutResponse(cancellationToken, RileyLinkCommandType.ResetRadioConfig);

            var commands = new List<(RileyLinkCommandType Command, byte[] Data)>();

            commands.Add((RileyLinkCommandType.SetSwEncoding, new byte[] { (byte)RileyLinkSoftwareEncoding.None }));
            commands.Add((RileyLinkCommandType.SetPreamble, new byte[] { 0x55, 0x55 }));

            foreach (var register in GetParameters(Entity.Options))
                commands.Add((RileyLinkCommandType.UpdateRegister, new[] { (byte)register.Item1, (byte)register.Item2 })) ;

            var responses = await SendCommandsAndGetResponses(cancellationToken, TimeSpan.FromSeconds(15), commands);
            if (responses.Any(r => r.Type != RileyLinkResponseType.OK))
                throw new OmniCoreRadioException(FailureType.RadioErrorResponse, "Failed to configure RileyLink");

            var (resultType, resultData) = await SendCommandGetResponse(cancellationToken, TimeSpan.FromSeconds(2), RileyLinkCommandType.GetState);
            if (resultType != RileyLinkResponseType.OK || resultData.Length != 2 || resultData[0] != 'O' || resultData[1] != 'K')
                throw new OmniCoreRadioException(FailureType.RadioErrorResponse, "RL status is not 'OK'");
            
            Activity = RadioActivity.Idle;
            IsConfigured = true;
        }

        private async Task<List<(RileyLinkResponseType Type, byte[] Data)>> SendCommandsAndGetResponses(CancellationToken cancellationToken,
            TimeSpan expectedProcessingDuration, List<(RileyLinkCommandType Command, byte[] Data)> commandList)
        {
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

            using var responseTimeout = new CancellationTokenSource(Entity.Options.RadioResponseTimeout);
            using var linkedCancellation =
                CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, responseTimeout.Token);
            //Debug.WriteLine($"{DateTimeOffset.Now} RL: Writing {BitConverter.ToString(dataArray)} Count: {commandList.Count}");
            await Peripheral.WriteToCharacteristic(
                RileyLinkServiceUuid, RileyLinkDataCharacteristicUuid,
                dataArray, linkedCancellation.Token);
            //Debug.WriteLine($"{DateTimeOffset.Now} RL: Written {BitConverter.ToString(dataArray)}");
            for (int i = 0; i < commandList.Count; i++)
            {
                resultList.Add(await GetResponse(cancellationToken, expectedProcessingDuration));
            }

            return resultList;
        }

        private async Task SendCommandWithoutResponse(CancellationToken cancellationToken,
            RileyLinkCommandType cmd, byte[] cmdData = null)
        {
            var data = GetCommandData(cmd, cmdData);
            using var responseTimeout = new CancellationTokenSource(Entity.Options.RadioResponseTimeout);
            using var linkedCancellation =
                CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, responseTimeout.Token);
            //Debug.WriteLine($"{DateTimeOffset.Now} RL: Writing noresponse {BitConverter.ToString(data)}");
            await Peripheral.WriteToCharacteristicWithoutResponse(
                RileyLinkServiceUuid, RileyLinkDataCharacteristicUuid,
                data, linkedCancellation.Token);
            //Debug.WriteLine($"{DateTimeOffset.Now} RL: Written noresponse {BitConverter.ToString(data)}");
        }

        private async Task<(RileyLinkResponseType Type, byte[] Data)> SendCommandGetResponse(CancellationToken cancellationToken, TimeSpan expectedProcessingDuration, RileyLinkCommandType cmd, byte[] cmdData = null)
        {
            var data = GetCommandData(cmd, cmdData);

            using var responseTimeout = new CancellationTokenSource(Entity.Options.RadioResponseTimeout);
            using var linkedCancellation =
                CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, responseTimeout.Token);
            //Debug.WriteLine($"{DateTimeOffset.Now} RL: Writing {BitConverter.ToString(data)}");
            await Peripheral.WriteToCharacteristic(
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

        private async Task<(RileyLinkResponseType Type, byte[] Data)> GetResponse(CancellationToken cancellationToken, TimeSpan expectedProcessingDuration)
        {
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
    }
}
