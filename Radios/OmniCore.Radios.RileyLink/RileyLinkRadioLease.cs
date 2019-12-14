using OmniCore.Model.Exceptions;
using OmniCore.Model.Interfaces;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
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
        private RileyLinkRadioConfiguration ActiveConfiguration = null;

        private readonly ISignalStrengthRepository SignalStrengthRepository;
        private readonly IRadioEventRepository RadioEventRepository;

        public RileyLinkRadioLease(
            ISignalStrengthRepository signalStrengthRepository,
            IRadioEventRepository radioEventRepository)
        {
            SignalStrengthRepository = signalStrengthRepository;
            RadioEventRepository = radioEventRepository;
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
                    radioConfiguration = await Radio.GetDefaultConfiguration();
            }

            var configuration = radioConfiguration as RileyLinkRadioConfiguration;
            if (configuration == null)
                throw new OmniCoreWorkflowException(FailureType.InvalidParameter, "Invalid radio configuration");

            using var connectCts = new CancellationTokenSource(configuration.RadioConnectTimeout);
            await PeripheralLease.Connect(configuration.KeepConnected, connectCts.Token);

            if (ActiveConfiguration.GetConfiguration().SequenceEqual(configuration.GetConfiguration()))
                return;

            await ConfigureRileyLink(configuration, cancellationToken);
            ActiveConfiguration = configuration;
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

        private async Task ConfigureRileyLink(RileyLinkRadioConfiguration radioConfiguration, CancellationToken cancellationToken)
        {
            await SendCommand(cancellationToken, RileyLinkCommandType.ResetRadioConfig);
            await SendCommand(cancellationToken, RileyLinkCommandType.SetSwEncoding, new byte[] { (byte)RileyLinkSoftwareEncoding.None });
            await SendCommand(cancellationToken, RileyLinkCommandType.SetPreamble, new byte[] { 0x55, 0x55 });

            var registers = radioConfiguration.GetConfiguration();
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
    }
}
