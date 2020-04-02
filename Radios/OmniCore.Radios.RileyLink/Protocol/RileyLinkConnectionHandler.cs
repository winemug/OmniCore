using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Threading;
using System.Threading.Tasks;
using Nito.AsyncEx;
using Nito.AsyncEx.Synchronous;
using OmniCore.Model.Entities;
using OmniCore.Model.Enumerations;
using OmniCore.Model.Exceptions;
using OmniCore.Model.Interfaces.Services.Internal;
using OmniCore.Model.Utilities;
using OmniCore.Radios.RileyLink.Enumerations;
using AsyncLock = Nito.AsyncEx.AsyncLock;

namespace OmniCore.Radios.RileyLink.Protocol
{
    public class RileyLinkConnectionHandler : IDisposable
    {

        private IBlePeripheralConnection PeripheralConnection;
        public RadioOptions ConfiguredOptions;
        public RadioOptions RequestedOptions;

        private static readonly Guid RileyLinkServiceUuid = Guid.Parse("0235733b-99c5-4197-b856-69219c2a3845");
        private static readonly Guid RileyLinkDataCharacteristicUuid = Guid.Parse("c842e849-5028-42e2-867c-016adada9155");
        private static readonly Guid RileyLinkResponseCharacteristicUuid = Guid.Parse("6e6c7910-b89e-43a5-a0fe-50c5e2b81f4a");

        private readonly AsyncLock CharacteristicAccessLock;
        public ConcurrentQueue<RileyLinkCommand> CommandQueue { get; }
        public ConcurrentQueue<RileyLinkCommand> ResponseQueue { get; }

        public Task QueueProcessor;

        private ManualResetEventSlim NewRequestEvent;
        private ManualResetEventSlim DisposeEvent;

        private IDisposable NotificationSubscription;

        private RileyLinkConnectionHandler(IBlePeripheralConnection peripheralConnection,
            RadioOptions options)
        {
            PeripheralConnection = peripheralConnection;
            RequestedOptions = options;

            QueueProcessor = Task.CompletedTask;
            CommandQueue = new ConcurrentQueue<RileyLinkCommand>();
            ResponseQueue = new ConcurrentQueue<RileyLinkCommand>();
            CharacteristicAccessLock = new AsyncLock();

            NewRequestEvent = new ManualResetEventSlim();
            DisposeEvent = new ManualResetEventSlim();

            QueueProcessor = Task.Run(async () => await ProcessQueue());

            NotificationSubscription = peripheralConnection
                .WhenCharacteristicNotificationReceived(RileyLinkServiceUuid, RileyLinkResponseCharacteristicUuid)
                .Subscribe(async (_) =>
                {
                    // Logging.Debug($"RLR: {Address} Characteristic notification received");
                    using var notifyReadTimeout =
                        new CancellationTokenSource(RequestedOptions.RadioResponseTimeout);

                    byte[] responseData = null;
                    using (await CharacteristicAccessLock.LockAsync(notifyReadTimeout.Token))
                    {
                        responseData = await peripheralConnection.ReadFromCharacteristic(
                            RileyLinkServiceUuid,
                            RileyLinkDataCharacteristicUuid,
                            notifyReadTimeout.Token);
                    }

                    if (responseData  != null)
                    {
                        //Logging.Debug($"RLR: {Address} Characteristic notification read: {BitConverter.ToString(commandResponse)}");

                        while (ResponseQueue.TryDequeue(out RileyLinkCommand command))
                        {
                            if (!command.NeedsResponse)
                                continue;

                            command.ParseResponse(responseData);
                        }
                    }
                });
        }

        public static async Task<RileyLinkConnectionHandler> Connect(
            IBlePeripheral peripheral,
            RadioOptions options,
            CancellationToken cancellationToken)
        {

            using var connectionTimeoutOverall = new CancellationTokenSource(options.RadioConnectionOverallTimeout);
            using var linkedCancellation = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken,
                connectionTimeoutOverall.Token);
            try
            {
                // Logging.Debug($"RLR: {Address} Connecting..");
                var connection = await peripheral.GetConnection(options.AutoConnect, options.KeepConnected,
                    options.RadioDiscoveryTimeout, options.RadioConnectTimeout,
                    options.RadioCharacteristicsDiscoveryTimeout,
                    linkedCancellation.Token);

                // Logging.Debug($"RLR: {Address} Requesting rssi..");
                await peripheral.ReadRssi(cancellationToken);

                return new RileyLinkConnectionHandler(connection, options);
            }
            catch (Exception e)
            {
                // Logging.Debug($"RLR: {Address} Error while connecting:\n {e.AsDebugFriendly()}");

                throw new OmniCoreRadioException(FailureType.RadioGeneralError, inner: e);
                
            }
        }

        public async Task Configure(
            RadioOptions options,
            CancellationToken cancellationToken)
        {
            if (ConfiguredOptions != null && ConfiguredOptions.SameAs(options))
                return;

            await Reset();
            await Noop().ToTask(cancellationToken);

            await SetSwEncoding(RileyLinkSoftwareEncoding.None).ToTask(cancellationToken);
            await SetPreamble(0x5555).ToTask(cancellationToken);

            foreach (var re in GetParameters(options))
                await UpdateRegister(re.Register, (byte) re.Value).ToTask(cancellationToken);

            var state = await GetState().ToTask(cancellationToken);

            if (!state.StateOk)
                throw new OmniCoreRadioException(FailureType.RadioErrorResponse, "RL status is not 'OK'");

            ConfiguredOptions = options;
        }

        public void TriggerQueue()
        {
            NewRequestEvent.Set();
        }

        //public async Task FlushRequests()
        //{

        //}

        //public async Task BatchRequests()
        //{
        //    while (CommandQueue.TryDequeue(out RileyLinkCommand command))
        //    {
        //        switch (command.CommandType)
        //        {
        //            // interruptions
        //            case RileyLinkCommandType.GetPacket:
        //            case RileyLinkCommandType.SendAndListen:
        //            // variable responses
        //            case RileyLinkCommandType.GetVersion:
        //            // no response
        //            case RileyLinkCommandType.Reset:
        //            case RileyLinkCommandType.None:
        //                BatchQueue.Enqueue(command);
        //                break;

        //            default:
        //                break;
        //        }


        //        ResponseQueue.Enqueue(command);
        //    }
        //}

        private async Task ProcessQueue()
        {
            int waitResult = 0;
            while (waitResult == 0)
            {
                NewRequestEvent.Reset();
                while (CommandQueue.TryDequeue(out RileyLinkCommand command))
                {
                    ResponseQueue.Enqueue(command);
                    await SendCommand(command, CancellationToken.None);
                }
                waitResult = WaitHandle.WaitAny(new [] {NewRequestEvent.WaitHandle, DisposeEvent.WaitHandle});
            }
        }

        public IObservable<RileyLinkStateResponse> GetState()
        {
            return new RileyLinkCommand
            {
                CommandType = RileyLinkCommandType.GetState
            }.Submit<RileyLinkStateResponse>(this);
        }

        public IObservable<RileyLinkVersionResponse> GetVersion()
        {
            return new RileyLinkCommand
            {
                CommandType = RileyLinkCommandType.GetVersion
            }.Submit<RileyLinkVersionResponse>(this);
        }

        public IObservable<RileyLinkPacketResponse> GetPacket(
            byte channel,
            uint timeoutMilliseconds)
        {
            return new RileyLinkCommand
            {
                CommandType = RileyLinkCommandType.GetPacket,
                Parameters = new Bytes()
                    .Append(channel)
                    .Append(timeoutMilliseconds)
                    .ToArray()
            }.Submit<RileyLinkPacketResponse>(this);
        }

        public IObservable<RileyLinkResponse> SendPacket(
            byte channel,
            byte repeatCount,
            ushort delayMilliseconds,
            ushort preambleExtensionMilliseconds,
            byte[] data
        )
        {
            return new RileyLinkCommand
            {
                CommandType = RileyLinkCommandType.SendPacket,
                Parameters = new Bytes()
                    .Append(channel)
                    .Append(repeatCount)
                    .Append(delayMilliseconds)
                    .Append(preambleExtensionMilliseconds)
                    .Append(data)
                    .ToArray()
            }.Submit<RileyLinkResponse>(this);
        }

        public IObservable<RileyLinkPacketResponse> SendAndListen(
                byte sendChannel,
                byte sendRepeatCount,
                ushort sendRepeatDelayMilliseconds,
                ushort sendPreambleExtensionMilliseconds,
                byte listenChannel,
                uint listenTimeoutMilliseconds,
                byte listenRetryCount,
                byte[] data
            )
        {
            return new RileyLinkCommand
            {
                CommandType = RileyLinkCommandType.SendAndListen,
                Parameters = new Bytes()
                    .Append(sendChannel)
                    .Append(sendRepeatCount)
                    .Append(sendRepeatDelayMilliseconds)
                    .Append(listenChannel)
                    .Append(listenTimeoutMilliseconds)
                    .Append(listenRetryCount)
                    .Append(sendPreambleExtensionMilliseconds)
                    .Append(data)
                    .ToArray()
            }.Submit<RileyLinkPacketResponse>(this);
        }

        public IObservable<RileyLinkResponse> UpdateRegister(
            RileyLinkRegister register,
            byte value
        )
        {
            return new RileyLinkCommand
            {
                CommandType = RileyLinkCommandType.UpdateRegister,
                Parameters = new Bytes()
                    .Append((byte) register)
                    .Append(value)
                    .ToArray()
            }.Submit<RileyLinkResponse>(this);
        }

        public IObservable<RileyLinkResponse> Noop()
        {
            return new RileyLinkCommand
            {
                CommandType = RileyLinkCommandType.None,
            }.Submit<RileyLinkResponse>(this);
        }

        public Task<RileyLinkCommand> Reset()
        {
            return new RileyLinkCommand
            {
                CommandType = RileyLinkCommandType.Reset
            }.Submit(this);
        }

        public IObservable<RileyLinkResponse> Led(
            RileyLinkLed led,
            RileyLinkLedMode mode)
        {
            return new RileyLinkCommand
            {
                CommandType = RileyLinkCommandType.Led,
                Parameters = new Bytes()
                    .Append((byte) led)
                    .Append((byte) mode)
                    .ToArray()
            }.Submit<RileyLinkResponse>(this);
        }

        public IObservable<RileyLinkRegisterValueResponse> ReadRegister(
            RileyLinkRegister register)
        {
            return new RileyLinkCommand
            {
                CommandType = RileyLinkCommandType.ReadRegister,
                Parameters = new Bytes()
                    .Append((byte) register)
                    .ToArray()
            }.Submit<RileyLinkRegisterValueResponse>(this);
        }

        public IObservable<RileyLinkResponse> SetModeRegisters(
            byte registerMode)
        {
            return new RileyLinkCommand
            {
                CommandType = RileyLinkCommandType.SetModeRegisters,
                Parameters = new Bytes()
                    .Append(registerMode)
                    .ToArray()
            }.Submit<RileyLinkResponse>(this);
        }

        public IObservable<RileyLinkResponse> SetSwEncoding(
            RileyLinkSoftwareEncoding encoding)
        {
            return new RileyLinkCommand
            {
                CommandType = RileyLinkCommandType.SetSwEncoding,
                Parameters = new Bytes()
                    .Append((byte) encoding)
                    .ToArray()
            }.Submit<RileyLinkResponse>(this);
        }

        public IObservable<RileyLinkResponse> SetPreamble(
            ushort preamble)
        {
            return new RileyLinkCommand
            {
                CommandType = RileyLinkCommandType.SetPreamble,
                Parameters = new Bytes()
                    .Append(preamble)
                    .ToArray()
            }.Submit<RileyLinkResponse>(this);
        }

        public IObservable<RileyLinkResponse> ResetRadioConfig()
        {
            return new RileyLinkCommand
            {
                CommandType = RileyLinkCommandType.ResetRadioConfig
            }.Submit<RileyLinkResponse>(this);
        }

        public IObservable<RileyLinkStatisticsResponse> GetStatistics()
        {
            return new RileyLinkCommand
            {
                CommandType = RileyLinkCommandType.GetStatistics
            }.Submit<RileyLinkStatisticsResponse>(this);
        }

        private async Task SendCommand(RileyLinkCommand command, CancellationToken cancellationToken)
        {
            using var responseTimeout = new CancellationTokenSource(RequestedOptions.RadioResponseTimeout);
            using var linkedCancellation =
                CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, responseTimeout.Token);
            //Debug.WriteLine($"{DateTimeOffset.Now} RL: Writing {BitConverter.ToString(data)}");
            try
            {
                using (await CharacteristicAccessLock.LockAsync(linkedCancellation.Token))
                {
                    await PeripheralConnection.WriteToCharacteristic(
                        RileyLinkServiceUuid, RileyLinkDataCharacteristicUuid,
                        GetCommandData(command),
                        linkedCancellation.Token);
                }

                command.SetTransmissionResult(null);
            }
            catch (Exception e)
            {
                command.SetTransmissionResult(e);
            }
            //Debug.WriteLine($"{DateTimeOffset.Now} RL: Written {BitConverter.ToString(data)}");
        }

        private byte[] GetCommandData(RileyLinkCommand command)
        {
            byte[] data;
            if (command.Parameters == null)
            {
                data = new byte[] { 1, (byte) command.CommandType };
            }
            else
            {
                data = new byte[command.Parameters.Length + 2];
                data[0] = (byte)(command.Parameters.Length + 1);
                data[1] = (byte) command.CommandType;
                Buffer.BlockCopy(command.Parameters, 0, data, 2, command.Parameters.Length);
            }
            return data;
        }

        public void Dispose()
        {
            DisposeEvent.Set();
            QueueProcessor.WaitAndUnwrapException();
            NotificationSubscription?.Dispose();
            NotificationSubscription = null;
            PeripheralConnection?.Dispose();
            PeripheralConnection = null;

        }

        private List<(RileyLinkRegister Register, int Value)> GetParameters(RadioOptions configuration)
        {
            var registers = new List<(RileyLinkRegister Register, int Value)>();

            registers.Add((RileyLinkRegister.SYNC0, 0x5A));
            registers.Add((RileyLinkRegister.SYNC1, 0xA5));
            registers.Add((RileyLinkRegister.PKTLEN, 0x50));

            var frequency = (int)(433910000 / (24000000 / Math.Pow(2, 16)));
            frequency += configuration.FrequencyShift;
            registers.Add((RileyLinkRegister.FREQ0, frequency & 0xff));
            registers.Add((RileyLinkRegister.FREQ1, (frequency >> 8) & 0xff));
            registers.Add((RileyLinkRegister.FREQ2, (frequency >> 16) & 0xff));

            registers.Add((RileyLinkRegister.DEVIATN, 0x44));

            registers.Add((RileyLinkRegister.FREND0, 0x00));
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
            registers.Add((RileyLinkRegister.PATABLE0, amplification));

            registers.Add((RileyLinkRegister.FSCTRL0, 0x00));
            registers.Add((RileyLinkRegister.FSCTRL1, configuration.RxIntermediateFrequency));

            var pktctrl1 = configuration.PqeThreshold << 5;
            pktctrl1 &= 0xE0;

            var pktctrl0 = configuration.DataWhitening ? 0x40 : 0x00;

            registers.Add((RileyLinkRegister.PKTCTRL1, pktctrl1));
            registers.Add((RileyLinkRegister.PKTCTRL0, pktctrl0));

            var mcfg4 = configuration.FilterBWExponent << 6;
            mcfg4 |= configuration.FilterBWDecimationRatio << 4;
            mcfg4 &= 0xF0;
            mcfg4 |= 0x0A;
            registers.Add((RileyLinkRegister.MDMCFG4, mcfg4));
            registers.Add((RileyLinkRegister.MDMCFG3, 0xBC));

            var mcfg2 = configuration.PreambleCheckWithCarrierSense ? 0x06 : 0x02;
            registers.Add((RileyLinkRegister.MDMCFG2, mcfg2));

            var mcfg1 = configuration.ForwardErrorCorrection ? 0x80 : 0x00;
            mcfg1 |= configuration.TxPreambleCountSetting << 4;
            registers.Add((RileyLinkRegister.MDMCFG1, mcfg1));
            registers.Add((RileyLinkRegister.MDMCFG0, 0xF8));

            var mcsm0 = 0x18 | configuration.RxAttenuationLevel;
            registers.Add((RileyLinkRegister.MCSM0, mcsm0));

            registers.Add((RileyLinkRegister.MCSM0, mcsm0));

            registers.Add((RileyLinkRegister.FOCCFG, 0x17));
            registers.Add((RileyLinkRegister.FSCAL3, 0xE9));
            registers.Add((RileyLinkRegister.FSCAL2, 0x2A));
            registers.Add((RileyLinkRegister.FSCAL1, 0x00));
            registers.Add((RileyLinkRegister.FSCAL0, 0x1F));
            registers.Add((RileyLinkRegister.TEST1, 0x35));
            registers.Add((RileyLinkRegister.TEST0, 0x09));

            return registers;
        }

    }
}
