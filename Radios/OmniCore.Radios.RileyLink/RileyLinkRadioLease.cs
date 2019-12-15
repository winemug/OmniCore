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
using OmniCore.Model.Interfaces.Platform;
using OmniCore.Model.Enumerations;
using OmniCore.Model.Interfaces.Repositories;
using OmniCore.Model.Interfaces.Workflow;
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
        private AsyncLock NotifyReaderLock = null;

        private readonly Guid RileyLinkServiceUuid = Guid.Parse("0235733b-99c5-4197-b856-69219c2a3845");
        private readonly Guid RileyLinkDataCharacteristicUuid = Guid.Parse("c842e849-5028-42e2-867c-016adada9155");
        private readonly Guid RileyLinkResponseCharacteristicUuid = Guid.Parse("6e6c7910-b89e-43a5-a0fe-50c5e2b81f4a");

        private IRadioPeripheralCharacteristic DataCharacteristic;
        private IRadioPeripheralCharacteristic ResponseCharacteristic;

        private ConcurrentQueue<(byte? ResponseNo, byte[] Response)> Responses;
        private readonly AsyncManualResetEvent ResponseEvent;
        private int RileylinkCounter;

        private IRadioConfiguration ActiveConfiguration;
        private bool IsConfigured = false;

        private readonly ISignalStrengthRepository SignalStrengthRepository;
        private readonly IRadioEventRepository RadioEventRepository;
        private readonly IRadioRepository RadioRepository;

        private TaskCompletionSource<bool> ConnectionInitializionResult;

        public RileyLinkRadioLease(
            ISignalStrengthRepository signalStrengthRepository,
            IRadioEventRepository radioEventRepository,
            IRadioRepository radioRepository)
        {
            SignalStrengthRepository = signalStrengthRepository;
            RadioEventRepository = radioEventRepository;
            RadioRepository = radioRepository;
            Responses = new ConcurrentQueue<(byte?,byte[])>();
            ResponseEvent = new AsyncManualResetEvent();
            ConnectionInitializionResult = new TaskCompletionSource<bool>();
            NotifyReaderLock = new AsyncLock();
        }

        public async Task Identify(CancellationToken cancellationToken)
        {
            await Connect(cancellationToken);
            await SendCommands(cancellationToken, new List<(RileyLinkCommandType, byte[])>
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
        }

        public void Dispose()
        {
            ResponseNotifySubscription?.Dispose();
            ResponseNotifySubscription = null;
            ResponseCharacteristic = null;
            DataCharacteristic = null;
            IsConfigured = false;

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
            using var connectCts = new CancellationTokenSource(ActiveConfiguration.RadioConnectTimeout);
            cancellationToken.Register(() => connectCts.Cancel());
            await PeripheralLease.Connect(ActiveConfiguration.KeepConnected, connectCts.Token);
            cancellationToken.Register(() => {});
            await ConnectionInitializionResult.Task;
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
                try
                {
                    IsConfigured = false;
                    var radioEvent = RadioEventRepository.New();
                    radioEvent.Radio = Radio.Entity;
                    radioEvent.EventType = RadioEvent.Connect;
                    radioEvent.Success = true;
                    await RadioEventRepository.Create(radioEvent, CancellationToken.None);

                    using var cts1 = new CancellationTokenSource(ActiveConfiguration.RadioResponseTimeout);
                    var characteristics = await PeripheralLease.GetCharacteristics(RileyLinkServiceUuid,
                        new[] { RileyLinkResponseCharacteristicUuid, RileyLinkDataCharacteristicUuid }, cts1.Token);
                    if (characteristics == null || characteristics.Length != 2)
                    {
                        await PeripheralLease.Disconnect(cts1.Token);
                        throw new OmniCoreRadioException(FailureType.RadioUnknownError, "GATT characteristics not found");
                    }

                    ResponseNotifySubscription?.Dispose();
                    ResponseCharacteristic = characteristics.First(c => c.Uuid == RileyLinkResponseCharacteristicUuid);
                    DataCharacteristic = characteristics.First(c => c.Uuid == RileyLinkDataCharacteristicUuid);

                    using var cts2 = new CancellationTokenSource(ActiveConfiguration.RadioResponseTimeout);
                    var currentCounter = await ResponseCharacteristic.Read(cts2.Token).ConfigureAwait(true);
                    RileylinkCounter = currentCounter[0];
                    ResponseNotifySubscription = ResponseCharacteristic.WhenNotificationReceived().Subscribe(async (_) =>
                    {
                        //var notifyLock = await NotifyReaderLock.LockAsync().ConfigureAwait(true);
                        try
                        {
                            bool read = false;
                            while (!read)
                            {
                                using var notifyReadTimeout =
                                    new CancellationTokenSource(ActiveConfiguration.RadioResponseTimeout);
                                // var counterData = await ResponseCharacteristic.Read(notifyReadTimeout.Token).ConfigureAwait(true);
                                var counter = 0; //counterData?[0];
                                var commandResponse = await DataCharacteristic.Read(notifyReadTimeout.Token)
                                    .ConfigureAwait(true);
                                Debug.WriteLine($"Ble response {counter} {BitConverter.ToString(commandResponse)}");
                                if (commandResponse != null && commandResponse.Length > 0)
                                {
                                    read = true;
                                    Responses.Enqueue((0, commandResponse));
                                    ResponseEvent.Set();
                                }
                            }
                        }
                        finally
                        {
                            //notifyLock.Dispose();
                        }
                    });
                    ConnectionInitializionResult.TrySetResult(true);
                }
                catch (Exception e)
                {
                    ConnectionInitializionResult.TrySetException(e);
                }
            });

            ConnectionFailedSubscription = PeripheralLease.WhenConnectionFailed().Subscribe( async (err) =>
            {
                ConnectionInitializionResult = new TaskCompletionSource<bool>();
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
                ConnectionInitializionResult = new TaskCompletionSource<bool>();
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
            await SendCommandGetResponse(cancellationToken, RileyLinkCommandType.ResetRadioConfig);
            await SendCommandGetResponse(cancellationToken, RileyLinkCommandType.SetSwEncoding, new byte[] { (byte)RileyLinkSoftwareEncoding.None });
            await SendCommandGetResponse(cancellationToken, RileyLinkCommandType.SetPreamble, new byte[] { 0x55, 0x55 });

            var registers = GetParameters(radioConfiguration);
            foreach (var register in registers)
                await SendCommandGetResponse(cancellationToken, RileyLinkCommandType.UpdateRegister, new[] { (byte)register.Item1, (byte)register.Item2 });

            var (resultType, resultData) = await SendCommandGetResponse(cancellationToken, RileyLinkCommandType.GetState);
            if (resultType != RileyLinkResponseType.OK || resultData.Length != 2 || resultData[0] != 'O' || resultData[1] != 'K')
                throw new OmniCoreRadioException(FailureType.RadioStateError, "RL returned status not OK.");
        }

        private async Task<List<(RileyLinkResponseType Type, byte[] Data)>> SendCommands(CancellationToken cancellationToken,
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

            // var notifyLock = await NotifyReaderLock.LockAsync().ConfigureAwait(true);

            RileylinkCounter++;
            var commandCounter = RileylinkCounter %= 256;

            await DataCharacteristic.Write(dataArray, cancellationToken);

            for (int i = commandCounter; i < commandCounter + commandList.Count; i++)
            {
                var expectedCounter = i % 256;
                using var responseTimeout = new CancellationTokenSource(ActiveConfiguration.RadioResponseTimeout);
                var result = await GetResponse(expectedCounter, cancellationToken);
                resultList.Add(result);
            }


            return resultList;
        }

        private async Task<(RileyLinkResponseType Type, byte[] Data)> SendCommandGetResponse(CancellationToken cancellationToken, RileyLinkCommandType cmd, byte[] cmdData = null)
        {
            var data = GetCommandData(cmd, cmdData);
            int commandCounter = 0;

            //var notifyLock = await NotifyReaderLock.LockAsync().ConfigureAwait(true);
            try
            {
                RileylinkCounter++;
                commandCounter = RileylinkCounter %= 256;
                using var responseTimeout1 = new CancellationTokenSource(ActiveConfiguration.RadioResponseTimeout);
                await DataCharacteristic.Write(data, responseTimeout1.Token).ConfigureAwait(true);
            }
            finally
            {
                //notifyLock.Dispose();
            }

            using var responseTimeout2 = new CancellationTokenSource(ActiveConfiguration.RadioResponseTimeout);
            var response = await GetResponse(commandCounter, responseTimeout2.Token).ConfigureAwait(true);

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

        private async Task<(RileyLinkResponseType Type, byte[] Data)> GetResponse(int responseNumber, CancellationToken cancellationToken)
        {
            bool responseFound = false;
            (byte?, byte[]) result = ValueTuple.Create<byte?, byte[]>(null, null);
            while (!responseFound)
            {
                ResponseEvent.Reset();
                var itemsToPutBack = new List<(byte? ResponseNo, byte[] Response)>();
                while (Responses.TryDequeue(out result))
                {
                    //if (result.Item1 == responseNumber)
                    {
                        responseFound = true;
                        break;
                    }
                    itemsToPutBack.Add(result);
                }

                foreach(var item in itemsToPutBack)
                    Responses.Enqueue(item);

                if (!responseFound)
                {
                    await ResponseEvent.WaitAsync(cancellationToken);
                }
            }

            var resultData = result.Item2;

            if (resultData == null || resultData.Length == 0)
                throw new OmniCoreRadioException(FailureType.RadioDisconnectPrematurely, "No data received from RileyLink");

            var responseType = (RileyLinkResponseType)resultData[0];
            var response = new byte[resultData.Length - 1];
            Buffer.BlockCopy(resultData, 1, response, 0, response.Length);
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
                case TxPower.A0_Lowest:
                    amplification = 0x0E;
                    break;
                case TxPower.A1_VeryLow:
                    amplification = 0x1D;
                    break;
                case TxPower.A2_Low:
                    amplification = 0x34;
                    break;
                case TxPower.A3_BelowNormal:
                    amplification = 0x2C;
                    break;
                case TxPower.A4_Normal:
                    amplification = 0x60;
                    break;
                case TxPower.A5_High:
                    amplification = 0x84;
                    break;
                case TxPower.A6_VeryHigh:
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
