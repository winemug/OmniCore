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
        private TaskCompletionSource<IDevice> DeviceChangedSource;
        public CrossBlePeripheralLease ActiveLease;


        public CrossBleRadioPeripheral(IDevice bleDevice)
        {
            BleDevice = bleDevice;
            LeaseSemaphore = new SemaphoreSlim(1, 1);
            ActiveLease = null;
            DeviceChangedSource = new TaskCompletionSource<IDevice>();
        }

        public Guid PeripheralId => BleDevice.Uuid;

        public string PeripheralName => BleDevice.Name;

        public void SwitchToNewDevice(IDevice newBleDevice)
        {
            lock (this)
            {
                BleDevice = newBleDevice;
                var pendingSource = DeviceChangedSource;
                DeviceChangedSource = new TaskCompletionSource<IDevice>();
                pendingSource.TrySetResult(newBleDevice);
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

        public IObservable<IRadioPeripheral> WhenDeviceChanged()
        {
            return Observable.Create<IRadioPeripheral>(async (observer) =>
            {
                var cts = new CancellationTokenSource();
                var cancelTask = Task.Delay(-1, cts.Token);
                while (true)
                {
                    var newDeviceTask = DeviceChangedSource.Task;
                    var resultTask = await Task.WhenAny(cancelTask, newDeviceTask);
                    if (resultTask == cancelTask)
                        break;
                    if (await newDeviceTask != null)
                    {
                        observer.OnNext(this);
                    }
                }
                cts.Dispose();
                return Disposable.Create(() =>
                {
                    cts.Cancel();
                    cts.Dispose();
                });
            });
        }

        public IObservable<IRadioPeripheral> WhenDeviceLost()
        {
            return Observable.Create<IRadioPeripheral>(async (observer) =>
            {
                var cts = new CancellationTokenSource();
                var cancelTask = Task.Delay(-1, cts.Token);
                while (true)
                {
                    var newDeviceTask = DeviceChangedSource.Task;
                    var resultTask = await Task.WhenAny(cancelTask, newDeviceTask);
                    if (resultTask == cancelTask)
                        break;
                    if (await newDeviceTask == null)
                    {
                        observer.OnNext(this);
                    }
                }
                cts.Dispose();
                return Disposable.Create(() =>
                {
                    cts.Cancel();
                    cts.Dispose();
                });
            });
        }

        public IObservable<IRadioPeripheral> WhenConnected() =>
            BleDevice.WhenConnected().WrapAndConvert((_) => this);

        public IObservable<Exception> WhenConnectionFailed() =>
            BleDevice.WhenConnectionFailed().WrapAndConvert((e) => e);

        public IObservable<IRadioPeripheral> WhenDisconnected() =>
            BleDevice.WhenDisconnected().WrapAndConvert((_) => this);

        public async Task<bool> Connect(CancellationToken cancellationToken)
        {
            if (BleDevice.Status == ConnectionStatus.Connected)
                return true;

            var connected = BleDevice.WhenConnected().FirstAsync().ToTask();
            var failed = BleDevice.WhenConnectionFailed().FirstAsync().ToTask();
            var canceled = Task.Delay(-1, cancellationToken);
            if (BleDevice.Status == ConnectionStatus.Disconnecting)
            {
                var disconnected = BleDevice.WhenDisconnected().FirstAsync().ToTask();
                var disconnectWait = await Task.WhenAny(disconnected, failed, canceled);
                if (disconnectWait == canceled)
                {
                    return false;
                }
                else if (disconnectWait == failed)
                {
                    failed = BleDevice.WhenConnectionFailed().FirstAsync().ToTask();
                }
            }

            if (BleDevice.Status != ConnectionStatus.Connecting)
            {
                BleDevice.Connect(new ConnectionConfig { AndroidConnectionPriority = ConnectionPriority.High, AutoConnect = false });
            }

            var result = await Task.WhenAny(connected, failed, canceled);
            if (result == canceled)
            {
                BleDevice.CancelConnection();
            }
            return result == connected;
        }

        public async Task Disconnect(TimeSpan timeout)
        {
            if (BleDevice.Status == ConnectionStatus.Disconnected)
                return;

            //if (BleDevice.Status == ConnectionStatus.Connecting)
            //{
            //    var connected = BleDevice.WhenConnected().FirstAsync().ToTask();
            //    var failed = BleDevice.WhenConnectionFailed().FirstAsync().ToTask();
            //    var connectWait = await Task.WhenAny(connected, failed, timeoutTask);
            //    if (connectWait == failed || connectWait == timeoutTask)
            //        return;
            //}

            if (BleDevice.Status != ConnectionStatus.Disconnecting)
                BleDevice.CancelConnection();

            if (timeout == TimeSpan.Zero)
                return;

            var timeoutTask = Task.Delay(timeout);
            var disconnected = BleDevice.WhenDisconnected().FirstAsync().ToTask();
            await Task.WhenAny(disconnected, timeoutTask);
        }

        public async Task<int> ReadRssi()
        {
            return await BleDevice.ReadRssi();
        }

        public void Dispose()
        {
        }
    }
}
