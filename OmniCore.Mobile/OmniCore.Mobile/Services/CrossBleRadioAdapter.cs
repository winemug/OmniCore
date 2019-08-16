using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using OmniCore.Model.Interfaces;
using Plugin.BluetoothLE;

namespace OmniCore.Mobile.Services
{
    public class CrossBleRadioAdapter : IRadioAdapter
    {
        public CrossBleRadioAdapter()
        {
        }

        public async Task<bool> TryEnable()
        {
            if (CrossBleAdapter.Current.Status != AdapterStatus.PoweredOff)
                return false;
            try
            {
                await Task.Run(() => { CrossBleAdapter.Current.SetAdapterState(true); });
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> TryDisable()
        {
            if (CrossBleAdapter.Current.Status != AdapterStatus.PoweredOn)
                return false;
            try
            {
                await Task.Run(() => { CrossBleAdapter.Current.SetAdapterState(false); });
                return true;
            }
            catch
            {
                return false;
            }
        }

        public IObservable<IRadioPeripheral> ScanPeripherals(Guid serviceId)
        {
            return Observable.Create<IRadioPeripheral>((observer) =>
                {
                    var scan = CrossBleAdapter.Current
                        .Scan(new ScanConfig
                        {
                            ScanType = BleScanType.Balanced,
                            ServiceUuids = new List<Guid>() {serviceId},
                            AndroidUseScanBatching = true
                        }).Subscribe((scanResult) =>
                        {
                            observer.OnNext(new CrossBleRadioPeripheral(scanResult.Device, scanResult.Rssi));
                        });

                    return Disposable.Create( () => { scan.Dispose(); });
                }
            );
        }

        public async Task<IRadioPeripheral> GetPeripheral(Guid id)
        {
            IDevice device = null;
            int rssi = 0;
            try
            {
                device = await CrossBleAdapter.Current.GetKnownDevice(id);
            }
            catch {}

            if (device == null)
            {
                try
                {
                    var devices = await CrossBleAdapter.Current.GetConnectedDevices();
                    device = devices.FirstOrDefault(d => d.Uuid == id);
                }
                catch{ }
            }

            if (device == null)
            {
                var scan = CrossBleAdapter.Current
                    .ScanUntilDeviceFound(id).Subscribe((found) =>
                    {
                    });

            }
            if (device == null)
                return null;
            else
                return new CrossBleRadioPeripheral(device, rssi);
        }
    }
}
