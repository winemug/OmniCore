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
using Plugin.BluetoothLE;

namespace OmniCore.Client.Platform
{
    public class CrossBleRadioAdapter : IRadioAdapter
    {
        private AsyncLock PeripheralSearchLock;
        private AsyncLock AdapterManagementLock;
        private Dictionary<Guid, CrossBleRadioPeripheral> PeripheralCache;

        private readonly ICoreContainer<IServerResolvable> Container;
        private readonly ICoreApplicationFunctions ApplicationFunctions;

        public CrossBleRadioAdapter(ICoreContainer<IServerResolvable> container,
            ICoreApplicationFunctions applicationFunctions)
        {
            Container = container;
            ApplicationFunctions = applicationFunctions;
            PeripheralSearchLock = new AsyncLock();
            AdapterManagementLock = new AsyncLock();
            PeripheralCache = new Dictionary<Guid, CrossBleRadioPeripheral>();
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
                    throw new OmniCoreAdapterException(FailureType.AdapterNotEnabled);
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

        public IObservable<IRadioPeripheralResult> FindPeripherals(Guid serviceUuid)
        {
            return Observable.Create<IRadioPeripheralResult>(async (observer) =>
                {
                    var cts = new CancellationTokenSource();
                    var cancellationToken = cts.Token;
                    var searchLock = await PeripheralSearchLock.LockAsync(cancellationToken);
                    IDisposable bluetoothLock = null;
                    IDisposable scanSubscription = null;
                    try
                    {
                        await TryEnsureAdapterEnabled(cancellationToken);

                        bluetoothLock = ApplicationFunctions.BluetoothKeepAwake();
                        var searchResultDictionary = new Dictionary<Guid, IRadioPeripheralResult>();

                        var connectedDevices = await CrossBleAdapter.Current
                            .GetConnectedDevices().ToTask(cancellationToken);

                        foreach (var connectedDevice in connectedDevices.Where(d => d.IsConnected()))
                        {
                            if (searchResultDictionary.ContainsKey(connectedDevice.Uuid))
                                continue;

                            var peripheral = CacheGetPeripheral(connectedDevice, serviceUuid);
                            if (peripheral != null && peripheral.ServiceUuid == serviceUuid)
                            {
                                var connectedResult = new CrossBleResult
                                {
                                    Peripheral = CacheGetPeripheral(connectedDevice, serviceUuid)
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
                                ServiceUuids = new List<Guid>() { serviceUuid },
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
                                        Peripheral = CacheGetPeripheral(scanResult.Device, serviceUuid),
                                        Rssi = scanResult.Rssi
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
                        bluetoothLock?.Dispose();
                        searchLock?.Dispose();
                    }

                    return Disposable.Create(() =>
                    {
                        cts.Cancel();
                        cts.Dispose();
                        scanSubscription?.Dispose();
                        bluetoothLock?.Dispose();
                        searchLock?.Dispose();
                    });
                }
            );
        }

        public async Task<IRadioPeripheralResult> FindPeripheral(Guid peripheralUuid, Guid serviceUuid, CancellationToken cancellationToken)
        {
            return await FindPeripherals(serviceUuid).FirstAsync(p => p.Peripheral.Uuid == peripheralUuid);
        }

        private CrossBleRadioPeripheral CacheGetPeripheral(IDevice bleDevice, Guid serviceUuid)
        {
            CrossBleRadioPeripheral peripheral;
            if (PeripheralCache.ContainsKey(bleDevice.Uuid))
            {
                peripheral = PeripheralCache[bleDevice.Uuid];
                if (!ReferenceEquals(peripheral.BleDevice, bleDevice))
                {
                    peripheral.SetDevice(bleDevice, serviceUuid);
                }
            }
            else
            {
                peripheral = NewPeripheral(bleDevice, serviceUuid);
            }

            PeripheralCache[bleDevice.Uuid] = peripheral;
            return peripheral;
        }

        private CrossBleRadioPeripheral NewPeripheral(IDevice device, Guid serviceUuid)
        {
            var peripheral = Container.Get<IRadioPeripheral>() as CrossBleRadioPeripheral;
            peripheral.SetDevice(device, serviceUuid);
            return peripheral;
        }
    }
}
