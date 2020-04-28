using System;
using System.Collections.Generic;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Threading;
using System.Threading.Tasks;
using Nito.AsyncEx.Synchronous;
using OmniCore.Model.Entities;
using OmniCore.Model.Enumerations;
using OmniCore.Model.Exceptions;
using OmniCore.Model.Interfaces.Services;
using OmniCore.Model.Interfaces.Services.Internal;
using OmniCore.Model.Utilities.Extensions;
using Plugin.BluetoothLE;

namespace OmniCore.Client.Platform
{
    public class BlePeripheralConnection : IBlePeripheralConnection, ICompositeDisposableProvider
    {
        private readonly ILogger Logger;
        private Dictionary<(Guid ServiceUuid, Guid CharacteristicUuid), IGattCharacteristic> CharacteristicsDictionary;
        private IDevice Device;
        private bool StayConnected;
        private BlePeripheralOptions PeripheralOptions;
        private readonly IBlePeripheralAdapter PeripheralAdapter;
        private readonly CancellationTokenSource DisconnectCancellationSource;

        public BlePeripheralConnection(
            ILogger logger,
            IConfigurationService configurationService,
            IBlePeripheralAdapter peripheralAdapter)
        {
            Logger = logger;
            PeripheralAdapter = peripheralAdapter;
            PeripheralOptions = configurationService
                    .GetBlePeripheralOptions(CancellationToken.None)
                    .WaitAndUnwrapException();
            DisconnectCancellationSource = new CancellationTokenSource()
                .AutoDispose(this);
        }

        private IBlePeripheral Peripheral;

        public void Dispose()
        {
            if (!StayConnected && Device.IsConnected())
            {
                try
                {
                    Logger.Debug("Closing peripheral connection");
                    Device.CancelConnection();
                    //TODO: fix this when ble library needs gets its notifications in order
                    //Device.WhenStatusChanged().FirstAsync(s => s == ConnectionStatus.Disconnected)
                    //    .Timeout(PeripheralOptions.PeripheralDisconnectTimeout).Wait();
                    //Logger.Debug("Peripheral connection closed");
                }
                catch (Exception e)
                {
                    Logger.Warning("Failed to close connection, ignoring error.", e);
                }
                PeripheralAdapter.InvalidatePeripheralState(Peripheral);
            }
            CompositeDisposable.Dispose();
        }

        public async Task<byte[]> ReadFromCharacteristic(Guid serviceUuid, Guid characteristicUuid,
            CancellationToken cancellationToken)
        {
            try
            {
                using var cts = CombineWithDisconnection(cancellationToken);
                Logger.Debug($"BLEPC: {Device.Uuid.AsMacAddress()} Read from characteristic requested");
                var result = await GetCharacteristic(serviceUuid, characteristicUuid).Read().ToTask(cts.Token);
                Logger.Debug($"BLEPC: {Device.Uuid.AsMacAddress()} Read from characteristic result received");
                return result.Data;
            }
            catch (OperationCanceledException)
            {
                Logger.Warning(DisconnectCancellationSource.IsCancellationRequested
                    ? $"BLE GATT Operation canceled due to unexpected loss of connection."
                    : "BLE GATT Operation canceled per request");
                throw;
            }
            catch (Exception e)
            {
                PeripheralAdapter.InvalidatePeripheralState(Peripheral);
                Logger.Error($"BLE GATT Operation failed", e);
                throw;
            }
        }

        public async Task WriteToCharacteristic(Guid serviceUuid, Guid characteristicUuid, byte[] data,
            CancellationToken cancellationToken)
        {
            try
            {
                using var cts = CombineWithDisconnection(cancellationToken);
                Logger.Debug($"BLEPC: {Device.Uuid.AsMacAddress()} Write to characteristic requested");
                await GetCharacteristic(serviceUuid, characteristicUuid).Write(data).ToTask(cts.Token);
                Logger.Debug($"BLEPC: {Device.Uuid.AsMacAddress()} Write to characteristic finished");
            }
            catch (OperationCanceledException)
            {
                Logger.Warning(DisconnectCancellationSource.IsCancellationRequested
                    ? $"BLE GATT Operation canceled due to unexpected loss of connection."
                    : "BLE GATT Operation canceled per request");
                throw;
            }
            catch (Exception e)
            {
                PeripheralAdapter.InvalidatePeripheralState(Peripheral);
                Logger.Error($"BLE GATT Operation failed", e);
                throw;
            }
        }

        public async Task WriteToCharacteristicWithoutResponse(Guid serviceUuid, Guid characteristicUuid, byte[] data,
            CancellationToken cancellationToken)
        {
            try
            {
                using var cts = CombineWithDisconnection(cancellationToken);
                Logger.Debug(
                    $"BLEPC: {Device.Uuid.AsMacAddress()} Write to characteristic without response requested");
                await GetCharacteristic(serviceUuid, characteristicUuid).WriteWithoutResponse(data)
                    .ToTask(cts.Token);
                Logger.Debug(
                    $"BLEPC: {Device.Uuid.AsMacAddress()} Write to characteristic without response finished");
            }
            catch (OperationCanceledException)
            {
                Logger.Warning(DisconnectCancellationSource.IsCancellationRequested
                    ? $"BLE GATT Operation canceled due to unexpected loss of connection."
                    : "BLE GATT Operation canceled per request");
                throw;
            }
            catch (Exception e)
            {
                PeripheralAdapter.InvalidatePeripheralState(Peripheral);
                Logger.Error($"BLE GATT Operation failed", e);
                throw;
            }
        }

        public IObservable<byte[]> WhenCharacteristicNotificationReceived(Guid serviceUuid, Guid characteristicUuid)
        {
            return Observable.Create<byte[]>(observer =>
            {
                var characteristic = GetCharacteristic(serviceUuid, characteristicUuid);
                characteristic
                    .RegisterAndNotify()
                    .Select(r => r.Data)
                    .Subscribe(bytes =>
                    {
                        Logger.Debug($"BLEPC: {Device.Uuid.AsMacAddress()} Characteristic notification received");
                        observer.OnNext(bytes);
                    }, exception =>
                    {
                        PeripheralAdapter.InvalidatePeripheralState(Peripheral);
                        observer.OnError(exception);
                    }).AutoDispose(this);

                return Disposable.Empty;
            });
        }

        public void Initialize(
            IDevice device,
            Dictionary<(Guid ServiceUuid, Guid CharacteristicUuid), IGattCharacteristic> characteristicsDictionary,
            IDisposable communicationDisposable,
            IBlePeripheral peripheral,
            bool stayConnected)
        {
            Device = device;
            CharacteristicsDictionary = characteristicsDictionary;
            StayConnected = stayConnected;
            Peripheral = peripheral;
            communicationDisposable.AutoDispose(this);

            peripheral.WhenConnectionStateChanged()
                .Where(s => s != PeripheralConnectionState.Connected)
                .Subscribe(_ =>
                {
                    DisconnectCancellationSource.Cancel();
                }).AutoDispose(this);
        }

        private IGattCharacteristic GetCharacteristic(Guid serviceUuid, Guid characteristicUuid)
        {
            var characteristic = CharacteristicsDictionary[(serviceUuid, characteristicUuid)];
            if (characteristic == null)
                throw new OmniCorePeripheralException(FailureType.PeripheralGeneralError,
                    "Characteristic not found on peripheral");
            return characteristic;
        }

        private CancellationTokenSource CombineWithDisconnection(CancellationToken cancellationToken)
        {
            return CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, DisconnectCancellationSource.Token);
        }

        public CompositeDisposable CompositeDisposable { get; } = new CompositeDisposable();
    }
}