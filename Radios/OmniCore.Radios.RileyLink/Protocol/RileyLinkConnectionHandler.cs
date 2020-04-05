using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Threading;
using System.Threading.Tasks;
using OmniCore.Model.Entities;
using OmniCore.Model.Enumerations;
using OmniCore.Model.Exceptions;
using OmniCore.Model.Interfaces.Services;
using OmniCore.Model.Interfaces.Services.Internal;
using OmniCore.Model.Utilities;
using OmniCore.Model.Utilities.Extensions;
using OmniCore.Radios.RileyLink.Enumerations;

namespace OmniCore.Radios.RileyLink.Protocol
{
    public class RileyLinkConnectionHandler : IDisposable
    {
        private static readonly Guid RileyLinkServiceUuid = Guid.Parse("0235733b-99c5-4197-b856-69219c2a3845");

        private static readonly Guid RileyLinkDataCharacteristicUuid =
            Guid.Parse("c842e849-5028-42e2-867c-016adada9155");

        private static readonly Guid RileyLinkResponseCharacteristicUuid =
            Guid.Parse("6e6c7910-b89e-43a5-a0fe-50c5e2b81f4a");

        private readonly ICoreLoggingFunctions Logger;
        private readonly IBlePeripheral Peripheral;
        public RadioOptions ConfiguredOptions;

        private IDisposable NotificationSubscription;
        private IBlePeripheralConnection PeripheralConnection;
        public RadioOptions RequestedOptions;

        public RileyLinkConnectionHandler(
            ICoreLoggingFunctions logger,
            IBlePeripheral peripheral,
            IBlePeripheralConnection peripheralConnection,
            RadioOptions options)
        {
            Peripheral = peripheral;
            Logger = logger;
            PeripheralConnection = peripheralConnection;
            RequestedOptions = options;

            ResponseQueue = new ConcurrentQueue<IRileyLinkResponse>();

            NotificationSubscription = peripheralConnection
                .WhenCharacteristicNotificationReceived(RileyLinkServiceUuid, RileyLinkResponseCharacteristicUuid)
                .Subscribe(async _ =>
                {
                    Logger.Debug($"{Header} Characteristic notification received");
                    using var notifyReadTimeout =
                        new CancellationTokenSource(RequestedOptions.RadioResponseTimeout);

                    Logger.Debug($"{Header} Reading incoming response data");
                    byte[] responseData = null;
                    responseData = await peripheralConnection.ReadFromCharacteristic(
                        RileyLinkServiceUuid,
                        RileyLinkDataCharacteristicUuid,
                        notifyReadTimeout.Token);
                    Logger.Debug($"{Header} Response data read");

                    if (responseData != null)
                        while (ResponseQueue.TryDequeue(out var response))
                            if (!response.SkipParse)
                            {
                                Logger.Debug($"{Header} Parsing response");
                                try
                                {
                                    response.Parse(responseData);
                                }
                                catch (Exception e)
                                {
                                    Logger.Debug($"{Header} Error while parsing response!\n{e.AsDebugFriendly()}");
                                }

                                break;
                            }
                });
        }

        private string Header => $"RLCH: {Peripheral.PeripheralUuid.AsMacAddress()}";

        public ConcurrentQueue<IRileyLinkResponse> ResponseQueue { get; }

        public void Dispose()
        {
            NotificationSubscription?.Dispose();
            NotificationSubscription = null;
            PeripheralConnection?.Dispose();
            PeripheralConnection = null;
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

            await SetModeRegisters(RileyLinkRegisterMode.Rx, GetRxParameters(options));
            await SetModeRegisters(RileyLinkRegisterMode.Tx, GetTxParameters(options));

            var response = await GetState().ToTask(cancellationToken);

            if (!response.StateOk)
                throw new OmniCoreRadioException(FailureType.RadioErrorResponse, "RL status is not 'OK'");

            Logger.Debug($"{Header} Configuration complete");
            ConfiguredOptions = options;
        }

        public IObservable<RileyLinkStateResponse> GetState()
        {
            return WhenResponseReceived<RileyLinkStateResponse>(
                new RileyLinkCommand
                {
                    Type = RileyLinkCommandType.GetState
                });
        }

        public IObservable<RileyLinkVersionResponse> GetVersion()
        {
            return WhenResponseReceived<RileyLinkVersionResponse>(
                new RileyLinkCommand
                {
                    Type = RileyLinkCommandType.GetVersion
                });
        }

        public IObservable<RileyLinkPacketResponse> GetPacket(
            byte channel,
            uint timeoutMilliseconds)
        {
            return WhenResponseReceived<RileyLinkPacketResponse>(new RileyLinkCommand
            {
                Type = RileyLinkCommandType.GetPacket,
                Parameters = new Bytes()
                    .Append(channel)
                    .Append(timeoutMilliseconds)
                    .ToArray()
            });
        }

        public IObservable<RileyLinkDefaultResponse> SendPacket(
            byte channel,
            byte repeatCount,
            ushort delayMilliseconds,
            ushort preambleExtensionMilliseconds,
            byte[] data
        )
        {
            return WhenResponseReceived<RileyLinkDefaultResponse>(new RileyLinkCommand
            {
                Type = RileyLinkCommandType.SendPacket,
                Parameters = new Bytes()
                    .Append(channel)
                    .Append(repeatCount)
                    .Append(delayMilliseconds)
                    .Append(preambleExtensionMilliseconds)
                    .Append(data)
                    .ToArray()
            });
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
            return WhenResponseReceived<RileyLinkPacketResponse>(new RileyLinkCommand
            {
                Type = RileyLinkCommandType.SendAndListen,
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
            });
        }

        public IObservable<RileyLinkDefaultResponse> UpdateRegister(
            RileyLinkRegister register,
            byte value
        )
        {
            return WhenResponseReceived<RileyLinkDefaultResponse>(new RileyLinkCommand
            {
                Type = RileyLinkCommandType.UpdateRegister,
                Parameters = new Bytes()
                    .Append((byte) register)
                    .Append(value)
                    .ToArray()
            });
        }

        public IObservable<RileyLinkDefaultResponse> Noop()
        {
            return WhenResponseReceived<RileyLinkDefaultResponse>(new RileyLinkCommand
            {
                Type = RileyLinkCommandType.None
            });
        }

        public IObservable<IRileyLinkCommand> Reset()
        {
            return WhenCommandSent(new RileyLinkCommand
            {
                Type = RileyLinkCommandType.Reset
            });
        }

        public IObservable<RileyLinkDefaultResponse> Led(
            RileyLinkLed led,
            RileyLinkLedMode mode)
        {
            return WhenResponseReceived<RileyLinkDefaultResponse>(new RileyLinkCommand
            {
                Type = RileyLinkCommandType.Led,
                Parameters = new Bytes()
                    .Append((byte) led)
                    .Append((byte) mode)
                    .ToArray()
            });
        }

        public IObservable<RileyLinkRegisterValueResponse> ReadRegister(
            RileyLinkRegister register)
        {
            return WhenResponseReceived<RileyLinkRegisterValueResponse>(new RileyLinkCommand
            {
                Type = RileyLinkCommandType.ReadRegister,
                Parameters = new Bytes()
                    .Append((byte) register)
                    .ToArray()
            });
        }

        public IObservable<RileyLinkDefaultResponse> SetModeRegisters(
            RileyLinkRegisterMode registerMode,
            List<(RileyLinkRegister Register, int Value)> registers)
        {
            var p = new Bytes((byte) registerMode);
            foreach (var r in registers)
                p.Append((byte) r.Register).Append((byte) r.Value);

            return WhenResponseReceived<RileyLinkDefaultResponse>(new RileyLinkCommand
            {
                Type = RileyLinkCommandType.SetModeRegisters,
                Parameters = p.ToArray()
            });
        }

        public IObservable<RileyLinkDefaultResponse> SetSwEncoding(
            RileyLinkSoftwareEncoding encoding)
        {
            return WhenResponseReceived<RileyLinkDefaultResponse>(new RileyLinkCommand
            {
                Type = RileyLinkCommandType.SetSwEncoding,
                Parameters = new Bytes()
                    .Append((byte) encoding)
                    .ToArray()
            });
        }

        public IObservable<RileyLinkDefaultResponse> SetPreamble(
            ushort preamble)
        {
            return WhenResponseReceived<RileyLinkDefaultResponse>(new RileyLinkCommand
            {
                Type = RileyLinkCommandType.SetPreamble,
                Parameters = new Bytes()
                    .Append(preamble)
                    .ToArray()
            });
        }

        public IObservable<RileyLinkDefaultResponse> ResetRadioConfig()
        {
            return WhenResponseReceived<RileyLinkDefaultResponse>(new RileyLinkCommand
            {
                Type = RileyLinkCommandType.ResetRadioConfig
            });
        }

        public IObservable<RileyLinkStatisticsResponse> GetStatistics()
        {
            return WhenResponseReceived<RileyLinkStatisticsResponse>(new RileyLinkCommand
            {
                Type = RileyLinkCommandType.GetStatistics
            });
        }

        private IObservable<IRileyLinkCommand> WhenCommandSent(IRileyLinkCommand command)
        {
            return Observable.Create<IRileyLinkCommand>(async observer =>
            {
                try
                {
                    await SendCommand(command);
                    observer.OnNext(command);
                    observer.OnCompleted();
                }
                catch (Exception e)
                {
                    observer.OnError(e);
                    observer.OnCompleted();
                }

                return Disposable.Empty;
            });
        }

        private IObservable<T> WhenResponseReceived<T>(IRileyLinkCommand command)
            where T : IRileyLinkResponse, new()
        {
            return Observable.Create<T>(async observer =>
            {
                var response = new T();
                ResponseQueue.Enqueue(response);

                try
                {
                    await SendCommand(command);
                }
                catch (Exception e)
                {
                    response.SkipParse = true;
                    observer.OnError(e);
                    observer.OnCompleted();
                    return Disposable.Empty;
                }

                var responseSubscription = response.Observable.Subscribe(_ =>
                    {
                        observer.OnNext(response);
                        observer.OnCompleted();
                    }, exception =>
                    {
                        observer.OnError(exception);
                        observer.OnCompleted();
                    }
                );
                return responseSubscription;
            });
        }

        private async Task SendCommand(IRileyLinkCommand command)
        {
            using var timeout = new CancellationTokenSource(RequestedOptions.RadioResponseTimeout);
            try
            {
                Logger.Debug($"{Header} Sending command {command.Type}");
                await PeripheralConnection.WriteToCharacteristic(
                    RileyLinkServiceUuid, RileyLinkDataCharacteristicUuid,
                    GetCommandData(command),
                    timeout.Token);
                Logger.Debug($"{Header} Write complete");
            }
            catch (Exception e)
            {
                Logger.Debug($"{Header} Write failed, reporting failure.\n{e.AsDebugFriendly()}");
                throw;
            }
        }

        private byte[] GetCommandData(IRileyLinkCommand command)
        {
            byte[] data;
            if (command.Parameters == null)
            {
                data = new byte[] {1, (byte) command.Type};
            }
            else
            {
                data = new byte[command.Parameters.Length + 2];
                data[0] = (byte) (command.Parameters.Length + 1);
                data[1] = (byte) command.Type;
                Buffer.BlockCopy(command.Parameters, 0, data, 2, command.Parameters.Length);
            }

            return data;
        }

        private List<(RileyLinkRegister Register, int Value)> GetRxParameters(RadioOptions configuration)
        {
            var registers = new List<(RileyLinkRegister Register, int Value)>();

            registers.Add((RileyLinkRegister.SYNC0, 0x5A));
            registers.Add((RileyLinkRegister.SYNC1, 0xA5));
            registers.Add((RileyLinkRegister.PKTLEN, 0x50));

            var frequency = (int) (433910000 / (24000000 / Math.Pow(2, 16)));
            frequency += configuration.RxFrequencyShift;
            registers.Add((RileyLinkRegister.FREQ0, frequency & 0xff));
            registers.Add((RileyLinkRegister.FREQ1, (frequency >> 8) & 0xff));
            registers.Add((RileyLinkRegister.FREQ2, (frequency >> 16) & 0xff));

            registers.Add((RileyLinkRegister.DEVIATN, 0x44));

            registers.Add((RileyLinkRegister.FSCTRL0, 0x00));
            registers.Add((RileyLinkRegister.FSCTRL1, configuration.RxIntermediateFrequency));

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

            registers.Add((RileyLinkRegister.FOCCFG, 0x17));
            registers.Add((RileyLinkRegister.FSCAL3, 0xE9));
            registers.Add((RileyLinkRegister.FSCAL2, 0x2A));
            registers.Add((RileyLinkRegister.FSCAL1, 0x00));
            registers.Add((RileyLinkRegister.FSCAL0, 0x1F));
            registers.Add((RileyLinkRegister.TEST1, 0x35));
            registers.Add((RileyLinkRegister.TEST0, 0x09));

            return registers;
        }

        private List<(RileyLinkRegister Register, int Value)> GetTxParameters(RadioOptions configuration)
        {
            var registers = new List<(RileyLinkRegister Register, int Value)>();

            var frequency = (int) (433910000 / (24000000 / Math.Pow(2, 16)));
            frequency += configuration.TxFrequencyShift;
            registers.Add((RileyLinkRegister.FREQ0, frequency & 0xff));
            registers.Add((RileyLinkRegister.FREQ1, (frequency >> 8) & 0xff));
            registers.Add((RileyLinkRegister.FREQ2, (frequency >> 16) & 0xff));

            registers.Add((RileyLinkRegister.DEVIATN, 0x44));

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

            registers.Add((RileyLinkRegister.FOCCFG, 0x17));
            registers.Add((RileyLinkRegister.FSCAL3, 0xE9));
            registers.Add((RileyLinkRegister.FSCAL2, 0x2A));
            registers.Add((RileyLinkRegister.FSCAL1, 0x00));
            registers.Add((RileyLinkRegister.FSCAL0, 0x1F));
            registers.Add((RileyLinkRegister.TEST1, 0x35));
            registers.Add((RileyLinkRegister.TEST0, 0x09));

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
            return registers;
        }
    }
}