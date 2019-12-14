using OmniCore.Model.Exceptions;
using OmniCore.Model.Interfaces;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
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

namespace OmniCore.Radios.RileyLink
{
    public class RileyLinkRadioLease : IRadioLease
    {
        public IRadioPeripheralLease PeripheralLease { get; set; }
        public IRadio Radio { get; set; }

        private IDisposable ConnectedSubscription = null;
        private IDisposable ConnectionFailedSubscription = null;
        private IDisposable DisconnectedSubscription = null;
        private IDisposable ResponseNotifySubscription = null;

        private readonly Guid RileyLinkServiceUuid = Guid.Parse("0235733b-99c5-4197-b856-69219c2a3845");
        private readonly Guid RileyLinkDataCharacteristicUuid = Guid.Parse("c842e849-5028-42e2-867c-016adada9155");
        private readonly Guid RileyLinkResponseCharacteristicUuid = Guid.Parse("6e6c7910-b89e-43a5-a0fe-50c5e2b81f4a");

        private IRadioPeripheralCharacteristic DataCharacteristic;
        private IRadioPeripheralCharacteristic ResponseCharacteristic;

        private ConcurrentQueue<(byte? ResponseNo, byte[] Response)> Responses;
        private readonly AsyncManualResetEvent ResponseEvent;
        private IRadioConfiguration ActiveConfiguration = null;

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
            Responses = new ConcurrentQueue<(byte?,byte[])>();
            ResponseEvent = new AsyncManualResetEvent();
            SubscribeToConnectionStates();
        }

        public async Task Identify(CancellationToken cancellationToken)
        {
        }

        public async Task Configure(IRadioConfiguration radioConfiguration, CancellationToken cancellationToken)
        {
            if (radioConfiguration == null)
            {
                if (ActiveConfiguration == null)
                {
                    var configurationJson = Radio.Entity.ConfigurationJson;
                    if (string.IsNullOrEmpty(configurationJson))
                    {
                        radioConfiguration = await Radio.GetDefaultConfiguration();
                    }
                    else
                    {
                        radioConfiguration = JsonConvert.DeserializeObject<RadioConfiguration>(configurationJson);
                    }
                }
            }

            using var connectCts = new CancellationTokenSource(radioConfiguration.RadioConnectTimeout);
            await PeripheralLease.Connect(radioConfiguration.KeepConnected, connectCts.Token);

            if (GetParameters(ActiveConfiguration).SequenceEqual(GetParameters(radioConfiguration)))
                return;

            await RadioRepository.Update(Radio.Entity, cancellationToken);
            await ConfigureRileyLink(radioConfiguration, cancellationToken);
            ActiveConfiguration = radioConfiguration;
        }

        public async Task ExecuteRequest(IPodRequest request, CancellationToken cancellationToken)
        {
            if (!ActiveConfiguration.KeepConnected)
            {
                await PeripheralLease.Disconnect(TimeSpan.FromSeconds(3));
            }
        }

        public void Dispose()
        {
            ResponseNotifySubscription?.Dispose();
            ResponseNotifySubscription = null;
            ResponseCharacteristic = null;
            DataCharacteristic = null;

            ConnectedSubscription?.Dispose();
            ConnectionFailedSubscription?.Dispose();
            DisconnectedSubscription?.Dispose();
            PeripheralLease.Dispose();
        }

        private void SubscribeToConnectionStates()
        {
            ConnectedSubscription = PeripheralLease.WhenConnected().Subscribe( async (_) =>
            {
                var radioEvent = RadioEventRepository.New();
                radioEvent.Radio = Radio.Entity;
                radioEvent.EventType = RadioEvent.Connect;
                radioEvent.Success = true;
                await RadioEventRepository.Create(radioEvent, CancellationToken.None);

                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));
                var characteristics = await PeripheralLease.GetCharacteristics(RileyLinkServiceUuid,
                    new[] { RileyLinkResponseCharacteristicUuid, RileyLinkDataCharacteristicUuid }, cts.Token);
                if (characteristics == null || characteristics.Length != 2)
                {
                    await PeripheralLease.Disconnect(TimeSpan.MinValue);
                    throw new OmniCoreRadioException(FailureType.RadioUnknownError, "GATT characteristics not found");
                }

                ResponseNotifySubscription?.Dispose();
                ResponseCharacteristic = characteristics.First(c => c.Uuid == RileyLinkResponseCharacteristicUuid);
                DataCharacteristic = characteristics.First(c => c.Uuid == RileyLinkDataCharacteristicUuid);

                ResponseNotifySubscription = ResponseCharacteristic.WhenNotificationReceived().Subscribe(async (_) =>
                {
                    using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
                    var counterData = await ResponseCharacteristic.Read(cts.Token);
                    var counter = counterData?[0];
                    var commandResponse = await DataCharacteristic.Read(cts.Token);
                    Responses.Enqueue((counter, commandResponse));
                    ResponseEvent.Set();
                });
            });

            ConnectionFailedSubscription = PeripheralLease.WhenConnectionFailed().Subscribe( async (err) =>
            {
                ResponseNotifySubscription?.Dispose();
                ResponseNotifySubscription = null;
                ResponseCharacteristic = null;
                DataCharacteristic = null;

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

                var radioEvent = RadioEventRepository.New();
                radioEvent.Radio = Radio.Entity;
                radioEvent.EventType = RadioEvent.Disconnect;
                radioEvent.Success = true;
                await RadioEventRepository.Create(radioEvent, CancellationToken.None);
            });
        }

        private async Task ConfigureRileyLink(IRadioConfiguration radioConfiguration, CancellationToken cancellationToken)
        {
            await SendCommand(cancellationToken, RileyLinkCommandType.ResetRadioConfig);
            await SendCommand(cancellationToken, RileyLinkCommandType.SetSwEncoding, new byte[] { (byte)RileyLinkSoftwareEncoding.None });
            await SendCommand(cancellationToken, RileyLinkCommandType.SetPreamble, new byte[] { 0x55, 0x55 });

            var registers = GetParameters(radioConfiguration);
            foreach (var register in registers)
                await SendCommand(cancellationToken, RileyLinkCommandType.UpdateRegister, new[] { (byte)register.Item1, (byte)register.Item2 });

            var result = await SendCommand(cancellationToken, RileyLinkCommandType.GetState);
            if (result.Length != 2 || result[0] != 'O' || result[1] != 'K')
                throw new OmniCoreRadioException(FailureType.RadioStateError, "RL returned status not OK.");
        }

        private async Task<byte[]> SendCommand(CancellationToken cancellationToken, RileyLinkCommandType cmd, byte[] cmdData = null)
        {
            try
            {
                byte[] data;
                if (cmdData == null)
                {
                    data = new byte[] { 1, (byte)cmd };
                }
                else
                {
                    data = new byte[cmdData.Length + 2];
                    data[0] = (byte)(cmdData.Length + 1);
                    data[1] = (byte)cmd;
                    Buffer.BlockCopy(cmdData, 0, data, 2, cmdData.Length);
                }

                var result = await SendCommandAndGetResponse(data, cancellationToken);

                if (result == null || result.Length == 0)
                    throw new OmniCoreRadioException(FailureType.RadioDisconnectPrematurely, "RL returned no result");

                else if (result[0] == (byte)RileyLinkResponseType.OK
                    || result[0] == (byte)RileyLinkResponseType.Interrupted)
                {
                    if (result.Length > 1)
                    {
                        var response = new byte[result.Length - 1];
                        Buffer.BlockCopy(result, 1, response, 0, response.Length);
                        return response;
                    }
                    else
                        return null;
                }
                else if (result[0] == (byte)RileyLinkResponseType.Timeout)
                    throw new OmniCoreTimeoutException(FailureType.RadioRecvTimeout);
                else
                    throw new OmniCoreRadioException(FailureType.RadioUnknownError, $"RL returned error code {result[0]}");
            }
            catch (OmniCoreException) { throw; }
            catch (Exception e)
            {
                throw new OmniCoreRadioException(FailureType.RadioDisconnectPrematurely, "Error while sending a command via BLE", e);
            }
        }

        private async Task<byte[]> SendCommandAndGetResponse(byte[] dataToWrite, CancellationToken cancellationToken)
        {
            ResponseEvent.Reset();
            using var responseTimeout = new CancellationTokenSource(ActiveConfiguration.RadioResponseTimeout);
            await DataCharacteristic.Write(dataToWrite, cancellationToken);
            await ResponseEvent.WaitAsync(cancellationToken);
            (byte?, byte[]) result;
            while(Responses.TryDequeue(out result))
            {
            }
            return result.Item2;
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
