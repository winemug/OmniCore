using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using OmniCore.Client.Extensions;
using OmniCore.Mobile.Services;
using OmniCore.Model.Interfaces;
using Plugin.BluetoothLE;

namespace OmniCore.Client.Services
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

        public async Task<List<IRadioPeripheral>> GetConnectedPeripherals(Guid serviceId)
        {
            var connectedList = await CrossBleAdapter.Current.GetConnectedDevices(serviceId);

            var devices = await CrossBleAdapter.Current.GetConnectedDevices(serviceId);
            return devices.Select(d => new CrossBleRadioPeripheral(d)).ToList<IRadioPeripheral>();
        }

        public IObservable<IRadioPeripheralScanResult> ScanPeripherals(Guid serviceId)
        {
            return Observable.Create<IRadioPeripheralScanResult>((observer) =>
                {
                    var searchResults = new HashSet<Guid>();
                    var scan = CrossBleAdapter.Current
                        .Scan(new ScanConfig
                        {
                            ScanType = BleScanType.Balanced,
                            ServiceUuids = new List<Guid>() {serviceId},
                            AndroidUseScanBatching = false
                        }).Subscribe((scanResult) =>
                        {
                            if (!searchResults.Contains(scanResult.Device.Uuid))
                            {
                                searchResults.Add(scanResult.Device.Uuid);
                                var crossScanResult = new CrossBleScanResult
                                    {
                                        RadioPeripheral = new CrossBleRadioPeripheral(scanResult.Device),
                                        Rssi = scanResult.Rssi
                                        };
                                observer.OnNext(crossScanResult );
                            }
                        });

                    return Disposable.Create( () => { scan.Dispose(); });
                }
            );
        }

        public async Task<IRadioPeripheral> GetPeripheral(Guid id, CancellationToken cancellationToken)
        {
            try
            {
                var device = await CrossBleAdapter.Current.GetKnownDevice(id).RunAsyncWithTimeoutAndCancellation(TimeSpan.FromSeconds(20), cancellationToken);

                if (device == null)
                {
                    device = await CrossBleAdapter.Current.ScanUntilDeviceFound(id).RunAsyncWithTimeoutAndCancellation(TimeSpan.FromSeconds(20), cancellationToken);
                }

                if (device != null)
                    return new CrossBleRadioPeripheral(device);
            }
            catch (TaskCanceledException)
            {
            }

            return null;
        }


    }
}
