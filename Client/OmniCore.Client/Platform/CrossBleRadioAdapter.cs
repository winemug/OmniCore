using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AppCenter;
using Nito.AsyncEx;
using OmniCore.Model.Enumerations;
using OmniCore.Model.Exceptions;
using OmniCore.Model.Extensions;
using OmniCore.Model.Utilities;
using OmniCore.Model.Interfaces;
using OmniCore.Model.Interfaces.Platform;
using Plugin.BluetoothLE;
using OmniCore.Model.Interfaces.Services;
using Unity;

namespace OmniCore.Client.Platform
{
    public class CrossBleRadioAdapter : IRadioAdapter
    {
        private AsyncLock PeripheralSearchLock;
        private AsyncLock AdapterManagementLock;

        [Dependency]
        private ICoreApplicationServices CoreApplicationServices { get; set; }
        public CrossBleRadioAdapter()
        {
            PeripheralSearchLock = new AsyncLock();
            AdapterManagementLock = new AsyncLock();
        }

        public async Task TryEnsureAdapterEnabled(CancellationToken cancellationToken)
        {
            switch (CrossBleAdapter.Current.Status)
            {
                case AdapterStatus.PoweredOn:
                case AdapterStatus.Unsupported:
                case AdapterStatus.Unauthorized:
                    return;
                case AdapterStatus.PoweredOff:
                    if (CrossBleAdapter.Current.CanControlAdapterState())
                    {
                        if (await TryEnableAdapter(cancellationToken))
                            return;
                    }
                    throw new OmniCoreRadioException(FailureType.BleDisabled);
            }
        }

        public async Task<bool> TryEnableAdapter(CancellationToken cancellationToken)
        {
            using var adapterManagementLock = await AdapterManagementLock.LockAsync(cancellationToken);
            if (CrossBleAdapter.Current.Status == AdapterStatus.PoweredOn)
            {
                return true;
            }

            CrossBleAdapter.Current.SetAdapterState(true);

            await CrossBleAdapter.Current.WhenStatusChanged()
                .Where(s => s == AdapterStatus.PoweredOn)
                .FirstAsync()
                .ToTask(cancellationToken);

            return CrossBleAdapter.Current.Status == AdapterStatus.PoweredOn;
        }

        public async Task<bool> TryDisableAdapter(CancellationToken cancellationToken)
        {
            using var adapterManagementLock = await AdapterManagementLock.LockAsync(cancellationToken);
            
            if (CrossBleAdapter.Current.Status == AdapterStatus.PoweredOff)
                return true;
            
            CrossBleAdapter.Current.SetAdapterState(false);
            await CrossBleAdapter.Current.WhenStatusChanged()
                .Where(s => s == AdapterStatus.PoweredOff)
                .FirstAsync()
                .ToTask(cancellationToken);

            return CrossBleAdapter.Current.Status == AdapterStatus.PoweredOff;
        }

        public IObservable<IRadioPeripheralResult> FindPeripherals(Guid serviceId)
        {
            return Observable.Create<IRadioPeripheralResult>(async (observer) =>
                {
                    var cts = new CancellationTokenSource();
                    var cancellationToken = cts.Token;
                    var searchLock = await PeripheralSearchLock.LockAsync(cancellationToken);
                    IDisposable scanSubscription = null;
                    try
                    {
                        await TryEnsureAdapterEnabled(cancellationToken);

                        var searchResultDictionary = new Dictionary<Guid, IRadioPeripheralResult>();

                        var connectedDevices = await CrossBleAdapter.Current
                            .GetConnectedDevices();

                        foreach (var connectedDevice in connectedDevices.Where(d => d.IsConnected()))
                        {
                            if (searchResultDictionary.ContainsKey(connectedDevice.Uuid))
                                continue;

                            var service = await connectedDevice.GetKnownService(serviceId);
                            if (service != null)
                            {
                                var connectedResult = new CrossBleResult
                                {
                                    Peripheral = new CrossBleRadioPeripheral(connectedDevice)
                                };

                                searchResultDictionary.Add(connectedDevice.Uuid, connectedResult);
                                observer.OnNext(connectedResult);
                            }
                        }

                        if (CrossBleAdapter.Current.IsScanning)
                            CrossBleAdapter.Current.StopScan();

                        scanSubscription = CrossBleAdapter.Current
                            .Scan(new ScanConfig
                            {
                                ScanType = BleScanType.Balanced,
                                ServiceUuids = new List<Guid>() {serviceId},
                                AndroidUseScanBatching = false
                            })
                            .Subscribe((scanResult) =>
                            {
                                IRadioPeripheralResult crossBleResult;
                                if (searchResultDictionary.ContainsKey(scanResult.Device.Uuid))
                                {
                                    crossBleResult = searchResultDictionary[scanResult.Device.Uuid];
                                    crossBleResult.Peripheral.Rssi = scanResult.Rssi;
                                    if (!string.IsNullOrEmpty(scanResult.AdvertisementData.LocalName))
                                        crossBleResult.Peripheral.Name = scanResult.AdvertisementData.LocalName;
                                }
                                else
                                {
                                    var crossResult = new CrossBleResult
                                    {
                                        Peripheral = new CrossBleRadioPeripheral(scanResult.Device)
                                    };
                                    searchResultDictionary.Add(scanResult.Device.Uuid, crossResult);
                                    observer.OnNext(crossResult);
                                }
                            });
                    }
                    catch (Exception e)
                    {
                        scanSubscription?.Dispose();
                        observer.OnError(e);
                        searchLock?.Dispose();
                    }

                    return Disposable.Create(() =>
                    {
                        cts.Cancel();
                        cts.Dispose();
                        scanSubscription?.Dispose();
                        searchLock?.Dispose();
                    });
                }
            );
        }

        public async Task<IRadioPeripheralResult> FindPeripheral(Guid peripheralUuid, CancellationToken cancellationToken)
        {
            await TryEnsureAdapterEnabled(cancellationToken);
            using var searchLock = await PeripheralSearchLock.LockAsync(cancellationToken);

            var devices = await CrossBleAdapter.Current.GetConnectedDevices();
            var device = devices.FirstOrDefault(d => d.Uuid == peripheralUuid);
            if (device != null)
                return new CrossBleResult
                {
                    Peripheral = new CrossBleRadioPeripheral(device),
                    Rssi = await device.ReadRssi().ToTask(cancellationToken) 
                };

            var scanResult = await CrossBleAdapter.Current
                .Scan()
                .FirstAsync(r => r.Device.Uuid == peripheralUuid)
                .ToTask(cancellationToken);
            
            return new CrossBleResult
            {
                Peripheral = new CrossBleRadioPeripheral(scanResult.Device),
                Rssi = scanResult.Rssi
            };
        }
    }
}
