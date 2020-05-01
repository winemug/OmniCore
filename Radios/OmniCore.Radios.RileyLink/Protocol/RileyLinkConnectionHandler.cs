using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Reactive.Threading.Tasks;
using System.Threading;
using System.Threading.Tasks;
using Nito.AsyncEx;
using OmniCore.Model.Constants;
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
    public class RileyLinkConnectionHandler : IDisposable, ICompositeDisposableProvider
    {
        private readonly IConfigurationService ConfigurationService;
        private readonly ILogger Logger;
        private RadioOptions ConfiguredOptions;

        private IDisposable NotificationSubscription;
        private IBlePeripheralConnection PeripheralConnection;
        
        private readonly ISubject<(byte, byte[])> ResponseDataReceivedSubject;
        private readonly IObservable<(byte, byte[])> WhenResponseDataReceived;

        private byte? SessionNotificationCounter = null;
        private CancellationTokenSource RadioErrorCancellationSource;

        public RileyLinkConnectionHandler(
            ILogger logger,
            IConfigurationService configurationService)
        {
            ConfigurationService = configurationService;
            Logger = logger;
            ResponseQueue = new ConcurrentQueue<IRileyLinkResponse>();
            ResponseDataReceivedSubject = new Subject<(byte, byte[])>();
            WhenResponseDataReceived = ResponseDataReceivedSubject.AsObservable()
                .Replay(256);
        }

        public void Initialize(IBlePeripheral peripheral)
        {
            IDisposable notificationSubscription = null;
            peripheral.WhenConnectionStateChanged()
                .Where(s => s == PeripheralConnectionState.Connected)
                .Subscribe(async _ =>
                {
                    RadioErrorCancellationSource = new CancellationTokenSource();
                    
                    byte[] responseNumberData =  await PeripheralConnection.ReadFromCharacteristic(
                        Uuids.RileyLinkServiceUuid,
                        Uuids.RileyLinkResponseCharacteristicUuid,
                        CancellationToken.None);

                    if (responseNumberData.Length != 1)
                    {
                        Logger.Error("Response number data is of incorrect length");
                        RadioErrorCancellationSource?.Cancel();
                    }

                    var responseNumber = responseNumberData[0];
                    notificationSubscription = GetNotificationSubscription();
                }, exception =>
                {
                    RadioErrorCancellationSource?.Cancel();
                }).DisposeWith(this);
            
            peripheral.WhenConnectionStateChanged()
                .Where(s => s == PeripheralConnectionState.Disconnected)
                .Subscribe(_ =>
                {
                    RadioErrorCancellationSource?.Dispose();
                    RadioErrorCancellationSource = null;
                    
                    notificationSubscription?.Dispose();
                    notificationSubscription = null;
                }).DisposeWith(this);
        }
        
        private IDisposable GetNotificationSubscription()
        {
            return PeripheralConnection
                .WhenCharacteristicNotificationReceived(Uuids.RileyLinkServiceUuid, Uuids.RileyLinkResponseCharacteristicUuid)
                .Subscribe(async _ =>
                {
                    try
                    {
                        Logger.Debug($"Characteristic notification received");
                        byte[] responseNumberData =  await PeripheralConnection.ReadFromCharacteristic(
                            Uuids.RileyLinkServiceUuid,
                            Uuids.RileyLinkResponseCharacteristicUuid,
                            CancellationToken.None);
                        
                        if (responseNumberData.Length != 1)
                            throw new OmniCoreRadioException(FailureType.RadioGeneralError, "Response number data is of incorrect length");

                        var responseNumber = responseNumberData[0];
                        Logger.Debug($"Incoming response #{responseNumber}");

                        byte[] responseData = await PeripheralConnection.ReadFromCharacteristic(
                            Uuids.RileyLinkServiceUuid,
                            Uuids.RileyLinkDataCharacteristicUuid,
                            CancellationToken.None);
                        Logger.Debug($"Response received");
                        
                        ResponseDataReceivedSubject.OnNext((responseNumber, responseData));
                    }
                    catch (Exception e)
                    {
                        Logger.Error($"Error while processing RileyLink responses", e);
                        ResponseDataReceivedSubject.OnError(e);
                        RadioErrorCancellationSource?.Cancel();
                    }
                }, exception =>
                {
                    ResponseDataReceivedSubject.OnError(exception);
                    RadioErrorCancellationSource?.Cancel();
                });
        }

        public ConcurrentQueue<IRileyLinkResponse> ResponseQueue { get; }

        public void Dispose()
        {
            NotificationSubscription?.Dispose();
            NotificationSubscription = null;
            
            CompositeDisposable.Dispose();
        }

        public async Task Configure(
            RadioOptions options,
            CancellationToken cancellationToken)
        {
            Logger.Debug($"Configure requested");
            if (ConfiguredOptions != null && ConfiguredOptions.SameAs(options))
            {
                Logger.Debug($"Already configured");
                return;
            }

            await Noop(cancellationToken);

            await SetSwEncoding(options.UseHardwareEncoding ?
                RileyLinkSoftwareEncoding.Manchester : RileyLinkSoftwareEncoding.None,
                cancellationToken);
            
            await SetPreamble(0x5555, cancellationToken);

            await SetModeRegisters(RileyLinkRegisterMode.Rx, GetRxParameters(options), cancellationToken);
            await SetModeRegisters(RileyLinkRegisterMode.Tx, GetTxParameters(options), cancellationToken);

            var response = await GetState(cancellationToken);

            if (!response.StateOk)
                throw new OmniCoreRadioException(FailureType.RadioErrorResponse, "RL status is not 'OK'");

            Logger.Debug($"Configuration complete");
            ConfiguredOptions = options;
        }

        public Task<RileyLinkStateResponse> GetState(CancellationToken cancellationToken)
        {
            return GetResponse<RileyLinkStateResponse>(
                new RileyLinkCommand
                {
                    Type = RileyLinkCommandType.GetState
                }, cancellationToken);
        }

        public Task<RileyLinkVersionResponse> GetVersion(CancellationToken cancellationToken)
        {
            return GetResponse<RileyLinkVersionResponse>(
                new RileyLinkCommand
                {
                    Type = RileyLinkCommandType.GetVersion
                }, cancellationToken);
        }

        public Task<RileyLinkPacketResponse> GetPacket(
            byte channel,
            uint timeoutMilliseconds,
            CancellationToken cancellationToken)
        {
            return GetResponse<RileyLinkPacketResponse>(new RileyLinkCommand
            {
                Type = RileyLinkCommandType.GetPacket,
                Parameters = new Bytes()
                    .Append(channel)
                    .Append(timeoutMilliseconds)
                    .ToArray()
            }, cancellationToken);
        }

        public Task<RileyLinkStandardResponse> SendPacket(
            byte channel,
            byte repeatCount,
            ushort delayMilliseconds,
            ushort preambleExtensionMilliseconds,
            byte[] data,
            CancellationToken cancellationToken
        )
        {
            return GetResponse<RileyLinkStandardResponse>(new RileyLinkCommand
            {
                Type = RileyLinkCommandType.SendPacket,
                Parameters = new Bytes()
                    .Append(channel)
                    .Append(repeatCount)
                    .Append(delayMilliseconds)
                    .Append(preambleExtensionMilliseconds)
                    .Append(data)
                    .ToArray()
            }, cancellationToken);
        }

        public Task<RileyLinkPacketResponse> SendAndListen(
            byte sendChannel,
            byte sendRepeatCount,
            ushort sendRepeatDelayMilliseconds,
            ushort sendPreambleExtensionMilliseconds,
            byte listenChannel,
            uint listenTimeoutMilliseconds,
            byte listenRetryCount,
            byte[] data,
            CancellationToken cancellationToken
        )
        {
            return GetResponse<RileyLinkPacketResponse>(new RileyLinkCommand
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
            }, cancellationToken);
        }

        public Task<RileyLinkStandardResponse> UpdateRegister(
            RileyLinkRegister register,
            byte value,
            CancellationToken cancellationToken
        )
        {
            return GetResponse<RileyLinkStandardResponse>(new RileyLinkCommand
            {
                Type = RileyLinkCommandType.UpdateRegister,
                Parameters = new Bytes()
                    .Append((byte) register)
                    .Append(value)
                    .ToArray()
            }, cancellationToken);
        }

        public Task Noop(CancellationToken cancellationToken)
        {
            return SendCommand(new RileyLinkCommand
            {
                Type = RileyLinkCommandType.None
            }, cancellationToken);
        }

        public Task Reset(CancellationToken cancellationToken)
        {
            return SendCommand(new RileyLinkCommand
            {
                Type = RileyLinkCommandType.Reset
            }, cancellationToken);
        }

        public Task<RileyLinkStandardResponse> Led(
            RileyLinkLed led,
            RileyLinkLedMode mode,
            CancellationToken cancellationToken)
        {
            return GetResponse<RileyLinkStandardResponse>(new RileyLinkCommand
            {
                Type = RileyLinkCommandType.Led,
                Parameters = new Bytes()
                    .Append((byte) led)
                    .Append((byte) mode)
                    .ToArray()
            }, cancellationToken);
        }

        public Task<RileyLinkRegisterValueResponse> ReadRegister(
            RileyLinkRegister register, CancellationToken cancellationToken)
        {
            return GetResponse<RileyLinkRegisterValueResponse>(new RileyLinkCommand
            {
                Type = RileyLinkCommandType.ReadRegister,
                Parameters = new Bytes()
                    .Append((byte) register)
                    .ToArray()
            }, cancellationToken);
        }

        public Task<RileyLinkStandardResponse> SetModeRegisters(
            RileyLinkRegisterMode registerMode,
            List<(RileyLinkRegister Register, int Value)> registers,
            CancellationToken cancellationToken)
        {
            var p = new Bytes((byte) registerMode);
            foreach (var r in registers)
                p.Append((byte) r.Register).Append((byte) r.Value);

            return GetResponse<RileyLinkStandardResponse>(new RileyLinkCommand
            {
                Type = RileyLinkCommandType.SetModeRegisters,
                Parameters = p.ToArray()
            }, cancellationToken);
        }

        public Task<RileyLinkStandardResponse> SetSwEncoding(
            RileyLinkSoftwareEncoding encoding, CancellationToken cancellationToken)
        {
            return GetResponse<RileyLinkStandardResponse>(new RileyLinkCommand
            {
                Type = RileyLinkCommandType.SetSwEncoding,
                Parameters = new Bytes()
                    .Append((byte) encoding)
                    .ToArray()
            }, cancellationToken);
        }

        public Task<RileyLinkStandardResponse> SetPreamble(
            ushort preamble, CancellationToken cancellationToken)
        {
            return GetResponse<RileyLinkStandardResponse>(new RileyLinkCommand
            {
                Type = RileyLinkCommandType.SetPreamble,
                Parameters = new Bytes()
                    .Append(preamble)
                    .ToArray()
            }, cancellationToken);
        }

        public Task<RileyLinkStandardResponse> ResetRadioConfig(CancellationToken cancellationToken)
        {
            return GetResponse<RileyLinkStandardResponse>(new RileyLinkCommand
            {
                Type = RileyLinkCommandType.ResetRadioConfig
            }, cancellationToken);
        }

        public Task<RileyLinkStatisticsResponse> GetStatistics(CancellationToken cancellationToken)
        {
            return GetResponse<RileyLinkStatisticsResponse>(new RileyLinkCommand
            {
                Type = RileyLinkCommandType.GetStatistics
            }, cancellationToken);
        }

        private async Task<T> GetResponse<T>(IRileyLinkCommand command, CancellationToken cancellationToken)
            where T : IRileyLinkResponse, new()
        {
            var response = new T();
            try
            {
                await SendCommand(command, cancellationToken);
            }
            catch (Exception)
            {
                response.SkipParse = true;
                throw;
            }

            return await response.Observable.Cast<T>().ToTask(cancellationToken);
        }

        private async Task SendCommand(IRileyLinkCommand command, CancellationToken cancellationToken)
        {
            try
            {
                Logger.Debug($"Sending command {command.Type}");
                await PeripheralConnection.WriteToCharacteristic(
                    Uuids.RileyLinkServiceUuid, Uuids.RileyLinkDataCharacteristicUuid,
                    GetCommandData(command),
                    cancellationToken);
                Logger.Debug($"Write complete");
            }
            catch (TimeoutException)
            {
                Logger.Error($"Operation timed out");
                
            }
            catch (Exception e)
            {
                Logger.Error($"Operation failed", e);
                throw;
            }
        }

        private byte[] GetCommandData(IRileyLinkCommand command)
        {
            byte[] data;
            if (command.Type == RileyLinkCommandType.None)
            {
                data = new byte[] {0};
            }
            else if (command.Parameters == null)
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

        public CompositeDisposable CompositeDisposable { get; } = new CompositeDisposable();
    }
}