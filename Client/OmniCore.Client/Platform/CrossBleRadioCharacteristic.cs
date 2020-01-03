using OmniCore.Model.Utilities;
using OmniCore.Model.Interfaces;
using Plugin.BluetoothLE;
using System;
using System.Collections.Generic;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using OmniCore.Model.Extensions;

namespace OmniCore.Client.Platform
{
    public class CrossBleRadioCharacteristic : IRadioPeripheralCharacteristic
    {
        private IDevice Device;
        private IGattService Service;
        private IGattCharacteristic Characteristic;
        private IDisposable NotificationSubscription = null;

        public Guid Uuid => Characteristic.Uuid;

        public CrossBleRadioCharacteristic(IDevice device,
            IGattService service,
            IGattCharacteristic characteristic)
        {
            Device = device;
            Service = service;
            Characteristic = characteristic;
        }

        public IObservable<IRadioPeripheralCharacteristic> WhenNotificationReceived()
        {
            return Observable.Create<IRadioPeripheralCharacteristic>((observer) =>
            {
                NotificationSubscription = Characteristic.RegisterAndNotify()
                    .Subscribe((_) => observer.OnNext(this));

                return Disposable.Create(() =>
                {
                    NotificationSubscription?.Dispose();
                    NotificationSubscription = null;
                });
            });
        }

        public async Task WriteWithoutResponse(byte[] data, CancellationToken cancellationToken)
        {
            await Characteristic.WriteWithoutResponse(data).ToTask(cancellationToken);
        }

        public async Task Write(byte[] data, CancellationToken cancellationToken)
        {
            await Characteristic.Write(data).ToTask(cancellationToken);
        }

        public async Task<byte[]> Read(CancellationToken cancellationToken)
        {
            return (await Characteristic.Read().ToTask(cancellationToken)).Data;
        }

        public void Dispose()
        {
            if (NotificationSubscription != null)
            {
                NotificationSubscription.Dispose();
                NotificationSubscription = null;
            }
        }
    }
}
