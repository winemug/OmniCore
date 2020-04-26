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
    public class BlePeripheralConnection : IBlePeripheralConnection
    {
        private readonly ILogger Logger;
        private Dictionary<(Guid ServiceUuid, Guid CharacteristicUuid), IGattCharacteristic> CharacteristicsDictionary;
        private IDisposable CommunicationDisposable;
        private IDevice Device;
        private bool StayConnected;
        private List<IDisposable> Subscriptions;
        private BlePeripheralOptions PeripheralOptions;
        private readonly IBlePeripheralAdapter PeripheralAdapter;

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
        }

        private IBlePeripheral Peripheral;

        public void Dispose()
        {
            foreach (var subscription in Subscriptions)
                subscription.Dispose();

            Subscriptions.Clear();

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

            CommunicationDisposable?.Dispose();
            CommunicationDisposable = null;
        }

        public async Task<byte[]> ReadFromCharacteristic(Guid serviceUuid, Guid characteristicUuid,
            CancellationToken cancellationToken)
        {
            try
            {
                Logger.Debug($"BLEPC: {Device.Uuid.AsMacAddress()} Read from characteristic requested");
                var result = await GetCharacteristic(serviceUuid, characteristicUuid).Read().ToTask(cancellationToken);
                Logger.Debug($"BLEPC: {Device.Uuid.AsMacAddress()} Read from characteristic result received");
                return result.Data;
            }
            catch (Exception e)
            {
                PeripheralAdapter.InvalidatePeripheralState(Peripheral);
                throw;
            }
        }

        public async Task WriteToCharacteristic(Guid serviceUuid, Guid characteristicUuid, byte[] data,
            CancellationToken cancellationToken)
        {
            try
            {
                Logger.Debug($"BLEPC: {Device.Uuid.AsMacAddress()} Write to characteristic requested");
                await GetCharacteristic(serviceUuid, characteristicUuid).Write(data).ToTask(cancellationToken);
                Logger.Debug($"BLEPC: {Device.Uuid.AsMacAddress()} Write to characteristic finished");
            }
            catch (Exception e)
            {
                PeripheralAdapter.InvalidatePeripheralState(Peripheral);
                throw;
            }
        }

        public async Task WriteToCharacteristicWithoutResponse(Guid serviceUuid, Guid characteristicUuid, byte[] data,
            CancellationToken cancellationToken)
        {
            try
            {
                Logger.Debug(
                    $"BLEPC: {Device.Uuid.AsMacAddress()} Write to characteristic without response requested");
                await GetCharacteristic(serviceUuid, characteristicUuid).WriteWithoutResponse(data)
                    .ToTask(cancellationToken);
                Logger.Debug(
                    $"BLEPC: {Device.Uuid.AsMacAddress()} Write to characteristic without response finished");
            }
            catch (Exception e)
            {
                PeripheralAdapter.InvalidatePeripheralState(Peripheral);
                throw;
            }
        }

        public IObservable<byte[]> WhenCharacteristicNotificationReceived(Guid serviceUuid, Guid characteristicUuid)
        {
            return Observable.Create<byte[]>(observer =>
            {
                var characteristic = GetCharacteristic(serviceUuid, characteristicUuid);
                var subscription = characteristic
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
                    });
                Subscriptions.Add(subscription);

                return Disposable.Create(() =>
                {
                    Subscriptions.Remove(subscription);
                    subscription.Dispose();
                });
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
            CommunicationDisposable = communicationDisposable;
            StayConnected = stayConnected;
            Peripheral = peripheral;
            Subscriptions = new List<IDisposable>();
        }
        private IGattCharacteristic GetCharacteristic(Guid serviceUuid, Guid characteristicUuid)
        {
            var characteristic = CharacteristicsDictionary[(serviceUuid, characteristicUuid)];
            if (characteristic == null)
                throw new OmniCorePeripheralException(FailureType.PeripheralGeneralError,
                    "Characteristic not found on peripheral");
            return characteristic;
        }
    }
}