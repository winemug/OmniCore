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
using OmniCore.Model.Interfaces.Services;
using OmniCore.Model.Interfaces.Services.Internal;
using OmniCore.Model.Utilities;
using OmniCore.Model.Utilities.Extensions;
using OmniCore.Radios.RileyLink.Enumerations;
using AsyncLock = Nito.AsyncEx.AsyncLock;

namespace OmniCore.Radios.RileyLink.Protocol
{
    public class RileyLinkConnectionHandler : IDisposable
    {

        private IBlePeripheralConnection PeripheralConnection;
        private readonly ICoreLoggingFunctions Logger;
        private readonly IBlePeripheral Peripheral;
        private string Header => $"RLCH: {Peripheral.PeripheralUuid.AsMacAddress()}";
        public RadioOptions ConfiguredOptions;
        public RadioOptions RequestedOptions;

        private static readonly Guid RileyLinkServiceUuid = Guid.Parse("0235733b-99c5-4197-b856-69219c2a3845");
        private static readonly Guid RileyLinkDataCharacteristicUuid = Guid.Parse("c842e849-5028-42e2-867c-016adada9155");
        private static readonly Guid RileyLinkResponseCharacteristicUuid = Guid.Parse("6e6c7910-b89e-43a5-a0fe-50c5e2b81f4a");

        private readonly AsyncLock CharacteristicAccessLock;
        public ConcurrentQueue<IRileyLinkCommand> CommandQueue { get; }
        public ConcurrentQueue<IRileyLinkCommand> ResponseQueue { get; }

        public Task QueueProcessor;

        private ManualResetEventSlim NewRequestEvent;
        private ManualResetEventSlim DisposeEvent;

        private IDisposable NotificationSubscription;

        private RileyLinkConnectionHandler(
            ICoreLoggingFunctions logger,
            IBlePeripheral peripheral,
            IBlePeripheralConnection peripheralConnection,
            RadioOptions options)
        {
            Logger = logger;
            PeripheralConnection = peripheralConnection;
            RequestedOptions = options;

            QueueProcessor = Task.CompletedTask;
            CommandQueue = new ConcurrentQueue<IRileyLinkCommand>();
            ResponseQueue = new ConcurrentQueue<IRileyLinkCommand>();
            CharacteristicAccessLock = new AsyncLock();

            NewRequestEvent = new ManualResetEventSlim();
            DisposeEvent = new ManualResetEventSlim();

            QueueProcessor = Task.Run(async () => await ProcessQueue());

            NotificationSubscription = peripheralConnection
                .WhenCharacteristicNotificationReceived(RileyLinkServiceUuid, RileyLinkResponseCharacteristicUuid)
                .Subscribe(async (_) =>
                {
                    Logger.Debug($"{Header} Characteristic notification received");
                    using var notifyReadTimeout =
                        new CancellationTokenSource(RequestedOptions.RadioResponseTimeout);

                    byte[] responseData = null;
                    using (var caLock = await CharacteristicAccessLock.LockAsync(notifyReadTimeout.Token))
                    {
                        responseData = await peripheralConnection.ReadFromCharacteristic(
                            RileyLinkServiceUuid,
                            RileyLinkDataCharacteristicUuid,
                            notifyReadTimeout.Token);
                    }

                    if (responseData  != null)
                    {
                        Logger.Debug($"{Header} Characteristic notification read");

                        while (ResponseQueue.TryDequeue(out var command))
                        {
                            if (!command.HasResponse)
                            {
                                Logger.Debug($"{Header} Skipping result for command with no response");
                                continue;
                            }

                            Logger.Debug($"{Header} Parsing response for type {command.CommandType} and notifying observers");
                            command.ParseResponse(responseData);
                        }
                    }
                });
        }

        public static async Task<RileyLinkConnectionHandler> Connect(
            ICoreLoggingFunctions logger,
            IBlePeripheral peripheral,
            RadioOptions options,
            CancellationToken cancellationToken)
        {

            using var connectionTimeoutOverall = new CancellationTokenSource(options.RadioConnectionOverallTimeout);
            using var linkedCancellation = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken,
                connectionTimeoutOverall.Token);
            try
            {
                logger.Debug($"RLCH: {peripheral.PeripheralUuid.AsMacAddress()} Getting connection");
                var connection = await peripheral.GetConnection(options.AutoConnect, options.KeepConnected,
                    options.RadioDiscoveryTimeout, options.RadioConnectTimeout,
                    options.RadioCharacteristicsDiscoveryTimeout,
                    linkedCancellation.Token);

                //logging.Debug($"RLR: {Address} Requesting rssi..");
                //await peripheral.ReadRssi(cancellationToken);

                return new RileyLinkConnectionHandler(logger, peripheral, connection, options);
                
            }
            catch (Exception e)
            {
                logger.Debug($"RLCH: {peripheral.PeripheralUuid.AsMacAddress()} Error while connecting:\n {e.AsDebugFriendly()}");
                throw new OmniCoreRadioException(FailureType.RadioGeneralError, inner: e);
                
            }
        }

        public async Task Configure(
            RadioOptions options,
            CancellationToken cancellationToken)
        {
            Logger.Debug($"{Header} Configure requested");
            if (ConfiguredOptions != null && ConfiguredOptions.SameAs(options))
            {
                Logger.Debug($"{Header} Already configured");
                return;
            }

            await Noop().ToTask(cancellationToken);

            await SetSwEncoding(RileyLinkSoftwareEncoding.None).ToTask(cancellationToken);
            await SetPreamble(0x5555).ToTask(cancellationToken);

            foreach (var re in GetParameters(options))
                await UpdateRegister(re.Register, (byte) re.Value).ToTask(cancellationToken);

            var state = await GetState().ToTask(cancellationToken);

            if (!state.StateOk)
                throw new OmniCoreRadioException(FailureType.RadioErrorResponse, "RL status is not 'OK'");

            Logger.Debug($"{Header} Configuration complete");
            ConfiguredOptions = options;
        }

        public void TriggerQueue()
        {
            NewRequestEvent.Set();
        }

        private async Task ProcessQueue()
        {
            int waitResult = 0;
            while (waitResult == 0)
            {
                NewRequestEvent.Reset();
                while (CommandQueue.TryDequeue(out var command))
                {
                    ResponseQueue.Enqueue(command);
                    Logger.Debug($"{Header} Processing RL command {command.CommandType}");
                    await SendCommand(command, CancellationToken.None);
                }
                waitResult = WaitHandle.WaitAny(new [] {NewRequestEvent.WaitHandle, DisposeEvent.WaitHandle});
            }
        }

        public IObservable<RileyLinkStateResponse> GetState()
        {
            return new RileyLinkCommand<RileyLinkStateResponse>
            {
                CommandType = RileyLinkCommandType.GetState
            }.Submit(this);
        }

        public IObservable<RileyLinkVersionResponse> GetVersion()
        {
            return new RileyLinkCommand<RileyLinkVersionResponse>
            {
                CommandType = RileyLinkCommandType.GetVersion
            }.Submit(this);
        }

        public IObservable<RileyLinkPacketResponse> GetPacket(
            byte channel,
            uint timeoutMilliseconds)
        {
            return new RileyLinkCommand<RileyLinkPacketResponse>
            {
                CommandType = RileyLinkCommandType.GetPacket,
                Parameters = new Bytes()
                    .Append(channel)
                    .Append(timeoutMilliseconds)
                    .ToArray()
            }.Submit(this);
        }

        public IObservable<RileyLinkResponse> SendPacket(
            byte channel,
            byte repeatCount,
            ushort delayMilliseconds,
            ushort preambleExtensionMilliseconds,
            byte[] data
        )
        {
            return new RileyLinkCommand<RileyLinkResponse>
            {
                CommandType = RileyLinkCommandType.SendPacket,
                Parameters = new Bytes()
                    .Append(channel)
                    .Append(repeatCount)
                    .Append(delayMilliseconds)
                    .Append(preambleExtensionMilliseconds)
                    .Append(data)
                    .ToArray()
            }.Submit(this);
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
            return new RileyLinkCommand<RileyLinkPacketResponse>
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
            }.Submit(this);
        }

        public IObservable<RileyLinkResponse> UpdateRegister(
            RileyLinkRegister register,
            byte value
        )
        {
            return new RileyLinkCommand<RileyLinkResponse>
            {
                CommandType = RileyLinkCommandType.UpdateRegister,
                Parameters = new Bytes()
                    .Append((byte) register)
                    .Append(value)
                    .ToArray()
            }.Submit(this);
        }

        public IObservable<RileyLinkResponse> Noop()
        {
            return new RileyLinkCommand<RileyLinkResponse>
            {
                CommandType = RileyLinkCommandType.None,
            }.Submit(this);
        }

        public Task Reset()
        {
            return new RileyLinkCommand<RileyLinkResponse>
            {
                CommandType = RileyLinkCommandType.Reset
            }.SubmitNoResponse(this);
        }

        public IObservable<RileyLinkResponse> Led(
            RileyLinkLed led,
            RileyLinkLedMode mode)
        {
            return new RileyLinkCommand<RileyLinkResponse>
            {
                CommandType = RileyLinkCommandType.Led,
                Parameters = new Bytes()
                    .Append((byte) led)
                    .Append((byte) mode)
                    .ToArray()
            }.Submit(this);
        }

        public IObservable<RileyLinkRegisterValueResponse> ReadRegister(
            RileyLinkRegister register)
        {
            return new RileyLinkCommand<RileyLinkRegisterValueResponse>
            {
                CommandType = RileyLinkCommandType.ReadRegister,
                Parameters = new Bytes()
                    .Append((byte) register)
                    .ToArray()
            }.Submit(this);
        }

        public IObservable<RileyLinkResponse> SetModeRegisters(
            byte registerMode)
        {
            return new RileyLinkCommand<RileyLinkResponse>
            {
                CommandType = RileyLinkCommandType.SetModeRegisters,
                Parameters = new Bytes()
                    .Append(registerMode)
                    .ToArray()
            }.Submit(this);
        }

        public IObservable<RileyLinkResponse> SetSwEncoding(
            RileyLinkSoftwareEncoding encoding)
        {
            return new RileyLinkCommand<RileyLinkResponse>
            {
                CommandType = RileyLinkCommandType.SetSwEncoding,
                Parameters = new Bytes()
                    .Append((byte) encoding)
                    .ToArray()
            }.Submit(this);
        }

        public IObservable<RileyLinkResponse> SetPreamble(
            ushort preamble)
        {
            return new RileyLinkCommand<RileyLinkResponse>
            {
                CommandType = RileyLinkCommandType.SetPreamble,
                Parameters = new Bytes()
                    .Append(preamble)
                    .ToArray()
            }.Submit(this);
        }

        public IObservable<RileyLinkResponse> ResetRadioConfig()
        {
            return new RileyLinkCommand<RileyLinkResponse>
            {
                CommandType = RileyLinkCommandType.ResetRadioConfig
            }.Submit(this);
        }

        public IObservable<RileyLinkStatisticsResponse> GetStatistics()
        {
            return new RileyLinkCommand<RileyLinkStatisticsResponse>
            {
                CommandType = RileyLinkCommandType.GetStatistics
            }.Submit(this);
        }

        private async Task SendCommand(IRileyLinkCommand command, CancellationToken cancellationToken)
        {
            using var responseTimeout = new CancellationTokenSource(RequestedOptions.RadioResponseTimeout);
            using var linkedCancellation =
                CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, responseTimeout.Token);
            try
            {
                using (var caLock = await CharacteristicAccessLock.LockAsync(linkedCancellation.Token))
                {
                    Logger.Debug($"{Header} Ble write command {command.CommandType}");
                    await PeripheralConnection.WriteToCharacteristic(
                        RileyLinkServiceUuid, RileyLinkDataCharacteristicUuid,
                        GetCommandData(command),
                        linkedCancellation.Token);
                    Logger.Debug($"{Header} Write complete");
                }

                Logger.Debug($"{Header} Report transmission success");
                command.SetTransmissionResult(null);
            }
            catch (Exception e)
            {
                Logger.Debug($"{Header} Write failed, reporting failure.\n{e.AsDebugFriendly()}");
                command.SetTransmissionResult(e);
            }
        }

        private byte[] GetCommandData(IRileyLinkCommand command)
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
