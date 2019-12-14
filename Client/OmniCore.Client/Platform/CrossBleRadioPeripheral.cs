using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
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
using Xamarin.Forms.Internals;

namespace OmniCore.Client.Platform
{
    public class CrossBleRadioPeripheral : IRadioPeripheral
    {
        public IDevice BleDevice { get; private set; }

        private readonly AsyncLock LeaseLock;

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

        public async Task<int> ReadRssi(CancellationToken cancellationToken)
        {
            return await BleDevice.ReadRssi().ToTask(cancellationToken);
        }

        public async void Dispose()
        {
            await LeaseLock.LockAsync(CancellationToken.None);
        }
    }
}
