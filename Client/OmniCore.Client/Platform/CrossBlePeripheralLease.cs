using OmniCore.Model.Interfaces;
using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using OmniCore.Model.Enumerations;
using OmniCore.Model.Exceptions;
using OmniCore.Model.Extensions;
using OmniCore.Model.Interfaces.Services;
using Plugin.BluetoothLE;
using Xamarin.Forms.Internals;

namespace OmniCore.Client.Platform
{
    public class CrossBlePeripheralLease : IRadioPeripheralLease
    {
        private readonly CrossBleRadioPeripheral CrossPeripheral;
        private IDevice BleDevice => CrossPeripheral.BleDevice;
        public CrossBlePeripheralLease(CrossBleRadioPeripheral crossBleRadioPeripheral)
        {
            CrossPeripheral = crossBleRadioPeripheral;
        }

        public IObservable<IRadioPeripheralLease> WhenConnected() =>
            BleDevice.WhenConnected().WrapAndConvert((_) => this);

        public IObservable<Exception> WhenConnectionFailed() =>
            BleDevice.WhenConnectionFailed().WrapAndConvert((e) => e);

        public IObservable<IRadioPeripheralLease> WhenDisconnected() =>
            BleDevice.WhenDisconnected().WrapAndConvert((_) => this);

        public async Task Connect(bool autoConnect, CancellationToken cancellationToken)
        {
            if (BleDevice.Status == ConnectionStatus.Connected)
                return;

            var connected = BleDevice.WhenConnected().FirstAsync().ToTask(cancellationToken);
            var failed = BleDevice.WhenConnectionFailed().FirstAsync().ToTask(cancellationToken);

            if (BleDevice.Status == ConnectionStatus.Disconnecting)
            {
                var disconnected = BleDevice.WhenDisconnected().FirstAsync().ToTask(cancellationToken);
                await Task.WhenAny(disconnected, failed);
            }

            if (BleDevice.Status != ConnectionStatus.Connecting)
            {
                BleDevice.Connect(new ConnectionConfig { AndroidConnectionPriority = ConnectionPriority.High, AutoConnect = autoConnect });
            }

            var result = await Task.WhenAny(connected, failed);
            if (result == failed)
            {
                throw new OmniCorePeripheralException(FailureType.PeripheralGeneralError, "Connect failed", await failed);
            }
        }

        public async Task Disconnect(CancellationToken cancellationToken)
        {
            if (BleDevice.Status == ConnectionStatus.Disconnected)
                return;

            if (BleDevice.Status != ConnectionStatus.Disconnecting)
                BleDevice.CancelConnection();

            await BleDevice.WhenDisconnected().FirstAsync().ToTask(cancellationToken);
        }

        public async Task<IRadioPeripheralCharacteristic[]> GetCharacteristics(Guid serviceId, Guid[] characteristicIds, CancellationToken cancellationToken)
        {
            if (BleDevice == null || !BleDevice.IsConnected())
                return null;
            var service = await BleDevice.GetKnownService(serviceId).ToTask(cancellationToken);

            return await service.DiscoverCharacteristics()
                .Where(c => characteristicIds.IndexOf(c.Uuid) >= 0)
                .Select(c => new CrossBleRadioCharacteristic(BleDevice, service, c))
                .ToArray();
        }

        public void Dispose()
        {
            CrossPeripheral.ActiveLeaseLockDisposable.Dispose();
        }
    }
}
