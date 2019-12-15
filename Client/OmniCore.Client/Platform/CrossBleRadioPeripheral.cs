using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Nito.AsyncEx;
using OmniCore.Model.Utilities;
using OmniCore.Client.Platform;
using OmniCore.Model.Enumerations;
using OmniCore.Model.Exceptions;
using OmniCore.Model.Extensions;
using OmniCore.Model.Interfaces;
using Plugin.BluetoothLE;
using OmniCore.Model.Interfaces.Platform;
using OmniCore.Model.Interfaces.Repositories;
using Unity;
using Xamarin.Forms.Internals;

namespace OmniCore.Client.Platform
{
    public class CrossBleRadioPeripheral : IRadioPeripheral
    {
        public IDevice BleDevice { get; private set; }

        private readonly AsyncLock LeaseLock;
        private IDisposable RssiUpdateSubscription = null;

        public CrossBleRadioPeripheral(IDevice bleDevice)
        {
            BleDevice = bleDevice;
            LeaseLock = new AsyncLock();
        }

        public Guid PeripheralUuid => BleDevice.Uuid;
        public string PeripheralName => BleDevice.Name;

        public async Task<IRadioPeripheralLease> Lease(CancellationToken cancellationToken)
        {
            var leaseLock = await LeaseLock.LockAsync(cancellationToken);
            return new CrossBlePeripheralLease(BleDevice, leaseLock);
        }

        private TimeSpan? rssiUpdateTimeSpan = null;
        public TimeSpan? RssiUpdateTimeSpan
        {
            get => rssiUpdateTimeSpan;
            set
            {
                if (value == null)
                {
                    RssiUpdateSubscription?.Dispose();
                    RssiUpdateSubscription = null;
                }
                else
                {
                    RssiUpdateSubscription?.Dispose();
                    RssiUpdateSubscription = BleDevice.ReadRssiContinuously(value).Subscribe(rssi =>
                    {
                        Rssi = rssi;
                        RssiDate = DateTimeOffset.UtcNow;
                    });
                }

                rssiUpdateTimeSpan = value;
            }
        }

        private int? rssi;
        public int? Rssi { get => rssi;
            set
            {
                if (value != null)
                {
                    rssi = value;
                    RssiDate = DateTime.UtcNow;
                }
            }
        }
        public DateTimeOffset? RssiDate { get; private set; }
        public DateTimeOffset? LastSeen { get; private set; }
        public async void Dispose()
        {
            await LeaseLock.LockAsync(CancellationToken.None);
            RssiUpdateSubscription.Dispose();
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
