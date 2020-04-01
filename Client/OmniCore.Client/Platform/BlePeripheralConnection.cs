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
        private readonly ICoreLoggingFunctions Logging;

        private IDisposable CommunicationDisposable;
        private bool StayConnected;
        private IDevice Device;
        private Dictionary<(Guid ServiceUuid, Guid CharacteristicUuid), IGattCharacteristic> CharacteristicsDictionary;
        private List<IDisposable> Subscriptions;
        
        public BlePeripheralConnection(
            ICoreLoggingFunctions logging)
        {
            Logging = logging;
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
        
        public void Dispose()
        {
            if (!StayConnected)
            {
                Device.CancelConnection();
            }
            
            foreach(var subscription in Subscriptions)
                subscription.Dispose();
            
            Subscriptions.Clear();

            CommunicationDisposable?.Dispose();
            CommunicationDisposable = null;

        }

        public async Task<byte[]> ReadFromCharacteristic(Guid serviceUuid, Guid characteristicUuid, CancellationToken cancellationToken)
        {
            Logging.Debug($"BLEPC: {Device.Uuid.AsMacAddress()} Read from characteristic requested");
            var result = await GetCharacteristic(serviceUuid, characteristicUuid).Read().ToTask(cancellationToken); 
            Logging.Debug($"BLEPC: {Device.Uuid.AsMacAddress()} Read from characteristic result received");
            return result.Data;
        }

        public async Task WriteToCharacteristic(Guid serviceUuid, Guid characteristicUuid, byte[] data, CancellationToken cancellationToken)
        {
            Logging.Debug($"BLEPC: {Device.Uuid.AsMacAddress()} Write to characteristic requested");
            await GetCharacteristic(serviceUuid, characteristicUuid).Write(data).ToTask(cancellationToken);
            Logging.Debug($"BLEPC: {Device.Uuid.AsMacAddress()} Write to characteristic finished");
        }

        public async Task WriteToCharacteristicWithoutResponse(Guid serviceUuid, Guid characteristicUuid, byte[] data,
            CancellationToken cancellationToken)
        {
            Logging.Debug(
                $"BLEPC: {Device.Uuid.AsMacAddress()} Write to characteristic without response requested");
            await GetCharacteristic(serviceUuid, characteristicUuid).WriteWithoutResponse(data)
                .ToTask(cancellationToken);
            Logging.Debug(
                $"BLEPC: {Device.Uuid.AsMacAddress()} Write to characteristic without response finished");
        }

        public IObservable<byte[]> WhenCharacteristicNotificationReceived(Guid serviceUuid, Guid characteristicUuid)
        {
            Logging.Debug($"BLEPC: {Device.Uuid.AsMacAddress()} Characteristic notification received");
            var characteristic = GetCharacteristic(serviceUuid, characteristicUuid);

            return Observable.Create<byte[]>(async observer =>
            {
                var subscription = characteristic
                    .RegisterAndNotify()
                    .Select(r => r.Data)
                    .Subscribe(bytes =>
                    {
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
        
        private IGattCharacteristic GetCharacteristic(Guid serviceUuid, Guid characteristicUuid)
        {
            var characteristic = CharacteristicsDictionary[(serviceUuid, characteristicUuid)];
            if (characteristic == null)
                throw new OmniCorePeripheralException(FailureType.PeripheralGeneralError, "Characteristic not found on peripheral");
            return characteristic;
        }
    }
}