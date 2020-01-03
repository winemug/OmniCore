using OmniCore.Model.Exceptions;
using OmniCore.Model.Interfaces;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Nito.AsyncEx;
using OmniCore.Model.Enumerations;
using OmniCore.Model.Interfaces.Data.Repositories;
using OmniCore.Model.Utilities;

namespace OmniCore.Radios.RileyLink
{
    public class RileyLinkRadioLease : IRadioLease
    {
        public IRadioPeripheralLease PeripheralLease { get; set; }
        private IRadio RadioInternal;
        public IRadio Radio { get => RadioInternal;
            set
            {
                RadioInternal = value;
                ActiveConfiguration = value.GetConfiguration();
                IsConfigured = false;
            }
        }

        private IDisposable ConnectedSubscription = null;
        private IDisposable ConnectionFailedSubscription = null;
        private IDisposable DisconnectedSubscription = null;
        private IDisposable ResponseNotifySubscription = null;

        private readonly Guid RileyLinkServiceUuid = Guid.Parse("0235733b-99c5-4197-b856-69219c2a3845");
        private readonly Guid RileyLinkDataCharacteristicUuid = Guid.Parse("c842e849-5028-42e2-867c-016adada9155");
        private readonly Guid RileyLinkResponseCharacteristicUuid = Guid.Parse("6e6c7910-b89e-43a5-a0fe-50c5e2b81f4a");

        private IRadioPeripheralCharacteristic DataCharacteristic;
        private IRadioPeripheralCharacteristic ResponseCharacteristic;

        private ConcurrentQueue<byte[]> Responses;
        private readonly AsyncManualResetEvent ResponseEvent;

        private IRadioConfiguration ActiveConfiguration;
        private bool IsConfigured = false;

        private readonly ISignalStrengthRepository SignalStrengthRepository;
        private readonly IRadioEventRepository RadioEventRepository;
        private readonly IRadioRepository RadioRepository;

        public RileyLinkRadioLease(
            ISignalStrengthRepository signalStrengthRepository,
            IRadioEventRepository radioEventRepository,
            IRadioRepository radioRepository)
        {
            SignalStrengthRepository = signalStrengthRepository;
            RadioEventRepository = radioEventRepository;
            RadioRepository = radioRepository;
            Responses = new ConcurrentQueue<byte[]>();
            ResponseEvent = new AsyncManualResetEvent();
        }

        public async Task Identify(CancellationToken cancellationToken)
        {
            await Connect(cancellationToken);
            await SendCommandsAndGetResponses(cancellationToken, new List<(RileyLinkCommandType, byte[])>
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

        public async Task Initialize(CancellationToken cancellationToken)
        {
            await Connect(cancellationToken);

            if (IsConfigured)
            {
                var radioConfiguration = Radio.GetConfiguration();
                if (GetParameters(ActiveConfiguration).SequenceEqual(GetParameters(radioConfiguration)))
                    return;
                else
                    ActiveConfiguration = radioConfiguration;
            }
            await ConfigureRileyLink(ActiveConfiguration, cancellationToken);
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
            Radio.Activity = RadioActivity.Listening;
            var arguments = new Bytes((byte) 0).Append(timeoutMilliseconds);
            var result = await SendCommandGetResponse(cancellationToken, RileyLinkCommandType.GetPacket,
                arguments.ToArray());
            Radio.Activity = RadioActivity.Idle;
            await Disconnect(cancellationToken);
            if (result.Type != RileyLinkResponseType.OK)
                throw new OmniCoreRadioException(FailureType.RadioErrorResponse, $"RL returned: {result.Type}");

            return (result.Data[0], result.Data[1..]);
        }

        public void Dispose()
        {
            ResponseNotifySubscription?.Dispose();
            ResponseNotifySubscription = null;
            ResponseCharacteristic = null;
            DataCharacteristic = null;
            IsConfigured = false;
            Radio.InUse = false;

            ConnectedSubscription?.Dispose();
            ConnectedSubscription = null;
            ConnectionFailedSubscription?.Dispose();
            ConnectionFailedSubscription = null;
            DisconnectedSubscription?.Dispose();
            DisconnectedSubscription = null;
            PeripheralLease?.Dispose();
            PeripheralLease = null;
        }

        private async Task Connect(CancellationToken cancellationToken)
        {
            SubscribeToConnectionStates();
            using var connectionTimeout = new CancellationTokenSource(ActiveConfiguration.RadioConnectTimeout);
            using (var linkedCancellation =
                CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, connectionTimeout.Token))
            {
                await PeripheralLease.Connect(ActiveConfiguration.KeepConnected, linkedCancellation.Token);
            }

            using var responseTimeout = new CancellationTokenSource(ActiveConfiguration.RadioResponseTimeout);
            using (var linkedCancellation =
                CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, connectionTimeout.Token))
            {
                var characteristics = await PeripheralLease.GetCharacteristics(RileyLinkServiceUuid,
                    new[] { RileyLinkResponseCharacteristicUuid, RileyLinkDataCharacteristicUuid }, linkedCancellation.Token);

                if (characteristics == null || characteristics.Length != 2)
                {
                    await PeripheralLease.Disconnect(responseTimeout.Token);
                    throw new OmniCoreRadioException(FailureType.RadioGeneralError, "GATT characteristics not found");
                }

                ResponseNotifySubscription?.Dispose();
                ResponseCharacteristic = characteristics.First(c => c.Uuid == RileyLinkResponseCharacteristicUuid);
                DataCharacteristic = characteristics.First(c => c.Uuid == RileyLinkDataCharacteristicUuid);
            }

            ResponseNotifySubscription = ResponseCharacteristic.WhenNotificationReceived().Subscribe(async (_) =>
            {
                while (true)
                {
                    using var notifyReadTimeout =
                        new CancellationTokenSource(ActiveConfiguration.RadioResponseTimeout);
                    var commandResponse = await DataCharacteristic.Read(notifyReadTimeout.Token);

                    if (commandResponse != null && commandResponse.Length > 0)
                    {
                        //Debug.WriteLine($"{DateTimeOffset.Now} RL: Response {BitConverter.ToString(commandResponse)}");
                        Responses.Enqueue(commandResponse);
                        ResponseEvent.Set();
                        break;
                    }
                }
                try
                {
                }
                finally
                {
                    //notifyLock.Dispose();
                }
            });
        }

        private async Task Disconnect(CancellationToken cancellationToken)
        {
            SubscribeToConnectionStates();
            if (!ActiveConfiguration.KeepConnected)
            {
                await PeripheralLease.Disconnect(cancellationToken);
            }
        }


        private void SubscribeToConnectionStates()
        {
            if (ConnectedSubscription != null)
                return;

            ConnectedSubscription = PeripheralLease.WhenConnected().Subscribe( async (_) =>
            {
                IsConfigured = false;
                var radioEvent = RadioEventRepository.New();
                radioEvent.Radio = Radio.Entity;
                radioEvent.EventType = RadioEvent.Connect;
                radioEvent.Success = true;
                await RadioEventRepository.Create(radioEvent, CancellationToken.None);
            });

            ConnectionFailedSubscription = PeripheralLease.WhenConnectionFailed().Subscribe( async (err) =>
            {
                ResponseNotifySubscription?.Dispose();
                ResponseNotifySubscription = null;
                ResponseCharacteristic = null;
                DataCharacteristic = null;
                IsConfigured = false;

                var radioEvent = RadioEventRepository.New();
                radioEvent.Radio = Radio.Entity;
                radioEvent.EventType = RadioEvent.Connect;
                radioEvent.Success = false;
                await RadioEventRepository.Create(radioEvent, CancellationToken.None);
            });

            DisconnectedSubscription = PeripheralLease.WhenDisconnected().Subscribe( async (_) =>
            {
                ResponseNotifySubscription?.Dispose();
                ResponseNotifySubscription = null;
                ResponseCharacteristic = null;
                IsConfigured = false;

                var radioEvent = RadioEventRepository.New();
                radioEvent.Radio = Radio.Entity;
                radioEvent.EventType = RadioEvent.Disconnect;
                radioEvent.Success = true;
                await RadioEventRepository.Create(radioEvent, CancellationToken.None);
            });
        }

        private async Task ConfigureRileyLink(IRadioConfiguration radioConfiguration, CancellationToken cancellationToken)
        {
            Radio.Activity = RadioActivity.Configuring;
            await SendCommandWithoutResponse(cancellationToken, RileyLinkCommandType.Reset);
            await SendCommandGetResponse(cancellationToken, RileyLinkCommandType.None);
            //await SendCommandWithoutResponse(cancellationToken, RileyLinkCommandType.ResetRadioConfig);

            var commands = new List<(RileyLinkCommandType, byte[])>();

            commands.Add((RileyLinkCommandType.SetSwEncoding, new byte[] { (byte)RileyLinkSoftwareEncoding.None }));
            commands.Add((RileyLinkCommandType.SetPreamble, new byte[] { 0x55, 0x55 }));

            foreach (var register in GetParameters(radioConfiguration))
                commands.Add((RileyLinkCommandType.UpdateRegister, new[] { (byte)register.Item1, (byte)register.Item2 })) ;

            var responses = await SendCommandsAndGetResponses(cancellationToken, commands);
            if (responses.Any(r => r.Type != RileyLinkResponseType.OK))
                throw new OmniCoreRadioException(FailureType.RadioErrorResponse, "Failed to configure RileyLink");

            await Task.Delay(2000);
            var (resultType, resultData) = await SendCommandGetResponse(cancellationToken, RileyLinkCommandType.GetState);
            if (resultType != RileyLinkResponseType.OK || resultData.Length != 2 || resultData[0] != 'O' || resultData[1] != 'K')
                throw new OmniCoreRadioException(FailureType.RadioErrorResponse, "RL status is not 'OK'");
            
            Radio.Activity = RadioActivity.Idle;
            IsConfigured = true;
        }

        private async Task<List<(RileyLinkResponseType Type, byte[] Data)>> SendCommandsAndGetResponses(CancellationToken cancellationToken,
            List<(RileyLinkCommandType, byte[])> commandList)
        {
            var resultList = new List<(RileyLinkResponseType Type, byte[] Data)>();
            var data = new Bytes((byte) 0);
            foreach (var command in commandList)
            {
                data.Append((byte)command.Item1);
                if (command.Item2 != null)
                    data.Append(command.Item2);
            }

            var dataArray = data.ToArray();
            dataArray[0] = (byte) (dataArray.Length - 1);

            using var responseTimeout = new CancellationTokenSource(ActiveConfiguration.RadioResponseTimeout);
            using var linkedCancellation =
                CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, responseTimeout.Token);
            //Debug.WriteLine($"{DateTimeOffset.Now} RL: Writing {BitConverter.ToString(dataArray)} Count: {commandList.Count}");
            await DataCharacteristic.Write(dataArray, linkedCancellation.Token);
            //Debug.WriteLine($"{DateTimeOffset.Now} RL: Written {BitConverter.ToString(dataArray)}");
            for (int i = 0; i < commandList.Count; i++)
            {
                resultList.Add(await GetResponse(cancellationToken));
            }

            return resultList;
        }

        private async Task SendCommandWithoutResponse(CancellationToken cancellationToken,
            RileyLinkCommandType cmd, byte[] cmdData = null)
        {
            var data = GetCommandData(cmd, cmdData);
            using var responseTimeout = new CancellationTokenSource(ActiveConfiguration.RadioResponseTimeout);
            using var linkedCancellation =
                CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, responseTimeout.Token);
            //Debug.WriteLine($"{DateTimeOffset.Now} RL: Writing noresponse {BitConverter.ToString(data)}");
            await DataCharacteristic.WriteWithoutResponse(data, linkedCancellation.Token);
            //Debug.WriteLine($"{DateTimeOffset.Now} RL: Written noresponse {BitConverter.ToString(data)}");
        }

        private async Task<(RileyLinkResponseType Type, byte[] Data)> SendCommandGetResponse(CancellationToken cancellationToken, RileyLinkCommandType cmd, byte[] cmdData = null)
        {
            var data = GetCommandData(cmd, cmdData);

            using var responseTimeout = new CancellationTokenSource(ActiveConfiguration.RadioResponseTimeout);
            using var linkedCancellation =
                CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, responseTimeout.Token);
            //Debug.WriteLine($"{DateTimeOffset.Now} RL: Writing {BitConverter.ToString(data)}");
            await DataCharacteristic.Write(data, linkedCancellation.Token);
            //Debug.WriteLine($"{DateTimeOffset.Now} RL: Written {BitConverter.ToString(data)}");

            var response = await GetResponse(cancellationToken);

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

        private async Task<(RileyLinkResponseType Type, byte[] Data)> GetResponse(CancellationToken cancellationToken)
        {
            byte[] result = null;
            while (true)
            {
                ResponseEvent.Reset();
                if (Responses.IsEmpty)
                {
                    await ResponseEvent.WaitAsync(cancellationToken);
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

        private List<Tuple<RileyLinkRegister, int>> GetParameters(IRadioConfiguration configuration)
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
