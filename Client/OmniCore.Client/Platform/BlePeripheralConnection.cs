using System;
using System.Collections.Generic;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Threading;
using System.Threading.Tasks;
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

        public BlePeripheralConnection(
            ILogger logger)
        {
            Logger = logger;
        }

        public IBlePeripheral Peripheral { get; set; }

        public void Dispose()
        {
            foreach (var subscription in Subscriptions)
                subscription.Dispose();

            Subscriptions.Clear();

            if (!StayConnected)
            {
                Device.CancelConnection();
                try
                {
                    Logger.Debug("Closing peripheral connection");
                    Device.WhenStatusChanged().FirstAsync(s => s == ConnectionStatus.Disconnected)
                        .Timeout(TimeSpan.FromSeconds(3)).Wait();
                    Logger.Debug("Peripheral connection closed");
                }
                catch (Exception e)
                {
                    Logger.Warning("Failed to close connection, ignoring error.", e);
                }
            }

            CommunicationDisposable?.Dispose();
            CommunicationDisposable = null;
        }

        public async Task<byte[]> ReadFromCharacteristic(Guid serviceUuid, Guid characteristicUuid,
            CancellationToken cancellationToken)
        {
            Logger.Debug($"BLEPC: {Device.Uuid.AsMacAddress()} Read from characteristic requested");
            var result = await GetCharacteristic(serviceUuid, characteristicUuid).Read().ToTask(cancellationToken);
            Logger.Debug($"BLEPC: {Device.Uuid.AsMacAddress()} Read from characteristic result received");
            return result.Data;
        }

        public async Task WriteToCharacteristic(Guid serviceUuid, Guid characteristicUuid, byte[] data,
            CancellationToken cancellationToken)
        {
            Logger.Debug($"BLEPC: {Device.Uuid.AsMacAddress()} Write to characteristic requested");
            await GetCharacteristic(serviceUuid, characteristicUuid).Write(data).ToTask(cancellationToken);
            Logger.Debug($"BLEPC: {Device.Uuid.AsMacAddress()} Write to characteristic finished");
        }

        public async Task WriteToCharacteristicWithoutResponse(Guid serviceUuid, Guid characteristicUuid, byte[] data,
            CancellationToken cancellationToken)
        {
            Logger.Debug(
                $"BLEPC: {Device.Uuid.AsMacAddress()} Write to characteristic without response requested");
            await GetCharacteristic(serviceUuid, characteristicUuid).WriteWithoutResponse(data)
                .ToTask(cancellationToken);
            Logger.Debug(
                $"BLEPC: {Device.Uuid.AsMacAddress()} Write to characteristic without response finished");
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
            bool stayConnected)
        {
            Device = device;
            CharacteristicsDictionary = characteristicsDictionary;
            CommunicationDisposable = communicationDisposable;
            StayConnected = stayConnected;
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