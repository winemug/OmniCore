using OmniCore.Model.Utilities;
using OmniCore.Model.Interfaces;
using Plugin.BluetoothLE;
using System;
using System.Collections.Generic;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using OmniCore.Model.Extensions;
using OmniCore.Model.Interfaces.Platform;

namespace OmniCore.Client.Platform
{
    public class CrossBleRadioCharacteristic : IRadioPeripheralCharacteristic
    {
        private IDevice Device;
        private IGattService Service;
        private IGattCharacteristic Characteristic;
        private int NotificationSubscriptionCount;

        public CrossBleRadioCharacteristic(IDevice device,
            IGattService service,
            IGattCharacteristic characteristic)
        {
            Device = device;
            Service = service;
            Characteristic = characteristic;
            NotificationSubscriptionCount = 0;
        }

        public IObservable<IRadioPeripheralCharacteristic> WhenNotificationReceived()
        {
            return Observable.Create<IRadioPeripheralCharacteristic>(async (observer) =>
            {
                var subCount = Interlocked.Increment(ref NotificationSubscriptionCount);
                if (subCount == 1)
                {
                    await Characteristic.EnableNotifications();
                }

                var bleNotificationSubscription = Characteristic.WhenNotificationReceived()
                    .Subscribe((_) => observer.OnNext(this));

                return Disposable.Create(async () =>
                {
                    bleNotificationSubscription.Dispose();
                    subCount = Interlocked.Decrement(ref NotificationSubscriptionCount);
                    if (subCount == 0)
                    {
                        await Characteristic.DisableNotifications();
                    }
                });
            });
        }

        public async Task Write(byte[] data, TimeSpan timeout, CancellationToken cancellationToken)
        {
            await Characteristic.Write(data).ToTask(timeout, cancellationToken);
        }

        public async Task<byte[]> Read(TimeSpan timeout, CancellationToken cancellationToken)
        {
            var readResult = await Characteristic.Read().RunAsync(cancellationToken);
            return readResult.Data;
        }

        public void Dispose()
        {
            if (NotificationSubscriptionCount > 0)
            {
                Task.Run(async () => await Characteristic.DisableNotifications()).Wait();
            }
        }
    }
}
