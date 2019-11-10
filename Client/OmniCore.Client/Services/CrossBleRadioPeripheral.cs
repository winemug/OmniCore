using System;
using System.Collections.Generic;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using OmniCore.Client.Extensions;
using OmniCore.Client.Services;
using OmniCore.Model.Interfaces;
using Plugin.BluetoothLE;

namespace OmniCore.Client.Services
{
    public class CrossBleRadioPeripheral : IRadioPeripheral
    {
        public IDevice BleDevice { get; private set; }

        private SemaphoreSlim LeaseSemaphore;
        public CrossBlePeripheralLease ActiveLease;

        public CrossBleRadioPeripheral(IDevice bleDevice)
        {
            BleDevice = bleDevice;
            LeaseSemaphore = new SemaphoreSlim(1, 1);
            ActiveLease = null;
        }

        public Guid PeripheralId => BleDevice.Uuid;

        public string PeripheralName => BleDevice.Name;

        public void SwitchToNewDevice(IDevice newBleDevice)
        {
            if (BleDevice.IsConnected())
            {

            }
        }

        public async Task<CrossBlePeripheralLease> AcquireLease(CancellationToken cancellationToken)
        {
            await LeaseSemaphore.WaitAsync(cancellationToken);
            ActiveLease = new CrossBlePeripheralLease(this);
            return ActiveLease;
        }

        public void Release()
        {
            ActiveLease = null;
            LeaseSemaphore.Release();
        }

        public async Task<bool> IsConnected()
        {
            return BleDevice.IsConnected();
        }

        public IObservable<IRadioPeripheral> WhenConnected() =>
            BleDevice.WhenConnected().WrapAndConvert((_) => this);

        public IObservable<Exception> WhenConnectionFailed() =>
            BleDevice.WhenConnectionFailed().WrapAndConvert((e) => e);

        public IObservable<IRadioPeripheral> WhenDisconnected() =>
            BleDevice.WhenDisconnected().WrapAndConvert((_) => this);

        public async Task<bool> Connect()
        {
            var connected = BleDevice.WhenConnected().ToTask();
            var failed = BleDevice.WhenConnectionFailed().ToTask();
            BleDevice.Connect(new ConnectionConfig { AndroidConnectionPriority = ConnectionPriority.High, AutoConnect = true });
            var result = await Task.WhenAny(connected, failed);
            return result == connected;
        }

        public async Task Disconnect()
        {
            BleDevice?.CancelConnection();
            BleDevice = null;
        }

        public async Task<int> ReadRssi()
        {
            return await BleDevice.ReadRssi();
        }

        public void Dispose()
        {
            BleDevice?.CancelConnection();
            BleDevice = null;
        }
    }
}
