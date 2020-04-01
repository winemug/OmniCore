using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Reactive.Threading.Tasks;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AppCenter;
using Nito.AsyncEx;
using OmniCore.Model.Enumerations;
using OmniCore.Model.Exceptions;
using OmniCore.Model.Interfaces.Common;
using OmniCore.Model.Utilities;
using OmniCore.Model.Interfaces.Services;
using OmniCore.Model.Interfaces.Services.Internal;
using OmniCore.Model.Utilities.Extensions;
using Plugin.BluetoothLE;
using Xamarin.Forms.Internals;

namespace OmniCore.Client.Platform
{
    public class BlePeripheralAdapter : IBlePeripheralAdapter
    {
        private AsyncLock AdapterManagementLock;
        private ConcurrentDictionary<Guid, BlePeripheral> PeripheralCache;
        private ConcurrentDictionary<Guid, IDevice> DeviceCache;

        private readonly ICoreContainer<IServerResolvable> Container;
        private readonly ICoreNotificationFunctions NotificationFunctions;
        private readonly ICoreLoggingFunctions Logging;
        private readonly ICoreApplicationFunctions ApplicationFunctions;
        private readonly IErosRadioProvider[] ErosRadioProviders;
        private readonly List<Guid> ErosRadioServiceUuids;

        private readonly AsyncLock PeripheralConnectionLockProvider;
        private readonly BlePeripheralScanner Scanner;
        
        public IObservable<IBlePeripheralAdapter> WhenScanStarted { get; }
        public IObservable<IBlePeripheralAdapter> WhenScanFinished { get; }

        public BlePeripheralAdapter(ICoreContainer<IServerResolvable> container,
            ICoreApplicationFunctions applicationFunctions,
            ICoreNotificationFunctions notificationFunctions,
            ICoreLoggingFunctions loggingFunctions,
            IErosRadioProvider[] erosRadioProviders)
        {
            Container = container;
            ApplicationFunctions = applicationFunctions;
            NotificationFunctions = notificationFunctions;
            Logging = loggingFunctions;
            ErosRadioProviders = erosRadioProviders;
            
            AdapterManagementLock = new AsyncLock();
            PeripheralCache = new ConcurrentDictionary<Guid, BlePeripheral>();
            DeviceCache = new ConcurrentDictionary<Guid, IDevice>();
            
            CrossBleAdapter.AndroidConfiguration.ShouldInvokeOnMainThread =
                CrossBleAdapter.AndroidConfiguration.IsMainThreadSuggested;
            CrossBleAdapter.AndroidConfiguration.UseInternalSyncQueue = true;
            CrossBleAdapter.AndroidConfiguration.UseNewScanner = true;
            CrossBleAdapter.AndroidConfiguration.RefreshServices = true;

            ErosRadioServiceUuids = erosRadioProviders
                .Select(rp => rp.ServiceUuid).ToList();

            PeripheralConnectionLockProvider = new AsyncLock();
            Scanner = new BlePeripheralScanner(ErosRadioServiceUuids, loggingFunctions, applicationFunctions);
            WhenScanStarted = Scanner.WhenScanStateChanged.Where(s => s).Select(s => this);
            WhenScanFinished = Scanner.WhenScanStateChanged.Where(s => !s).Select(s => this);
        }

        public IObservable<IBlePeripheralAdapter> WhenAdapterDisabled()
        {
            return Observable.Create<IBlePeripheralAdapter>(observer =>
            {
                
                if (CrossBleAdapter.Current.Status == AdapterStatus.PoweredOff)
                {
                    Logging.Debug($"BLE: Adapter disabled");
                    observer.OnNext(this);
                }

                return CrossBleAdapter.Current.WhenStatusChanged()
                    .Where(s => s == AdapterStatus.PoweredOff)
                    .Subscribe(_ =>
                    {
                        Logging.Debug($"BLE: Adapter disabled");
                        observer.OnNext(this);
                    });
            });
        }

        public IObservable<IBlePeripheralAdapter> WhenAdapterEnabled()
        {
            return Observable.Create<IBlePeripheralAdapter>(observer =>
            {
                if (CrossBleAdapter.Current.Status == AdapterStatus.PoweredOn)
                {
                    Logging.Debug($"BLE: Adapter enabled");
                    observer.OnNext(this);                    
                }

                return CrossBleAdapter.Current.WhenStatusChanged()
                    .Where(s => s == AdapterStatus.PoweredOn)
                    .Subscribe(_ =>
                    {
                        Logging.Debug($"BLE: Adapter enabled");
                        observer.OnNext(this);
                    });
            });
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

            Logging.Debug($"BLE: Trying to enable adapter");
            CrossBleAdapter.Current.SetAdapterState(true);
            Logging.Debug($"BLE: Waiting for adapter to get enabled");

            await CrossBleAdapter.Current.WhenStatusChanged()
                .Where(s => s == AdapterStatus.PoweredOn)
                .FirstAsync()
                .ToTask(cancellationToken);

            Logging.Debug($"BLE: Adapter enabled successfully");
            return CrossBleAdapter.Current.Status == AdapterStatus.PoweredOn;
        }

        public async Task<IDisposable> PeripheralConnectionLock(CancellationToken cancellationToken)
        {
            var lockDisposable = await PeripheralConnectionLockProvider.LockAsync(cancellationToken);
            cancellationToken.ThrowIfCancellationRequested();
                
            await TryEnsureAdapterEnabled(cancellationToken);
            cancellationToken.ThrowIfCancellationRequested();

            Scanner.Pause();
            var bluetoothLock = ApplicationFunctions.BluetoothKeepAwake();
                
            return Disposable.Create(() =>
            {
                bluetoothLock.Dispose();
                Scanner.Resume();
                lockDisposable.Dispose();
            });
        }

        public IObservable<IBlePeripheral> FindErosRadioPeripherals()
        {
            return Observable.Create<IBlePeripheral>(async (observer) =>
                {
                    var cts = new CancellationTokenSource();
                    var cancellationToken = cts.Token;

                    var observedPeripheralUuids = new HashSet<Guid>();

                    IDisposable scanSubscription = null;

                    try
                    {
                        Logging.Debug($"BLE: Request connected devices");
                        using (var ppc= await PeripheralConnectionLock(cts.Token))
                        {
                            var connectedDevices = await CrossBleAdapter.Current
                                .GetConnectedDevices().ToTask(cancellationToken);
                            Logging.Debug($"BLE: Received connected devices");

                            foreach (var connectedDevice in connectedDevices)
                            {
                                var service= await connectedDevice.DiscoverServices()
                                    .FirstOrDefaultAsync(s => ErosRadioServiceUuids.Contains(s.Uuid));

                                if (service != null)
                                {
                                    DeviceCache[connectedDevice.Uuid] = connectedDevice;
                                    var peripheral = GetPeripheral(connectedDevice.Uuid, service.Uuid);
                                    peripheral.UpdateSubscriptions(connectedDevice);
                                    
                                    Logging.Debug(
                                        $"BLE: {peripheral.PeripheralUuid.AsMacAddress()} Notifying connected peripheral as found");
                                    observedPeripheralUuids.Add(peripheral.PeripheralUuid);
                                    observer.OnNext(peripheral);
                                }
                                cancellationToken.ThrowIfCancellationRequested();
                            }

                            var searchStart = DateTimeOffset.UtcNow;
                            var connectedPeripheralUuids = connectedDevices.Select(c => c.Uuid);
                            foreach (var peripheralUuid in DeviceCache.Keys.ToList())
                            {
                                if (!connectedPeripheralUuids.Any(cuuid => cuuid == peripheralUuid))
                                    DeviceCache[peripheralUuid] = null;
                                
                                var peripheral = GetPeripheral(peripheralUuid, ErosRadioServiceUuids[0]);
                                peripheral.DiscoveryState = (PeripheralDiscoveryState.Searching, searchStart);
                            }

                            Logging.Debug($"BLE: Connecting to scan observable");
                            scanSubscription = Scanner.Scan()
                                .Subscribe(async (scanResult) =>
                                {
                                    DeviceCache[scanResult.Device.Uuid] = scanResult.Device;
                                    var peripheral = GetPeripheral(scanResult.Device.Uuid,
                                        scanResult.AdvertisementData.ServiceUuids[0]) as BlePeripheral;

                                    peripheral.UpdateSubscriptions(scanResult.Device);
                                    
                                    if (string.IsNullOrEmpty(peripheral.Name))
                                        peripheral.Name = scanResult.AdvertisementData.LocalName;

                                    if (!string.IsNullOrEmpty(scanResult.Device.Name))
                                        peripheral.Name = scanResult.Device.Name;

                                    peripheral.Rssi = (scanResult.Rssi, DateTimeOffset.UtcNow);
                                    peripheral.DiscoveryState = (PeripheralDiscoveryState.Discovered, DateTimeOffset.UtcNow);
                                    
                                    if (!observedPeripheralUuids.Contains(peripheral.PeripheralUuid))
                                    {
                                        Logging.Debug($"BLE: {peripheral.PeripheralUuid.AsMacAddress()} Reporting found peripheral");
                                        observedPeripheralUuids.Add(peripheral.PeripheralUuid);
                                        observer.OnNext(peripheral);
                                    }
                                    
                                });
                        }
                    }
                    catch (Exception e)
                    {
                        Logging.Debug($"BLE: Error during scan: \n {e.AsDebugFriendly()}");
                        cts.Cancel();
                        cts.Dispose();
                        scanSubscription?.Dispose();
                        var dateFinished = DateTimeOffset.UtcNow;
                        foreach (var peripheral in PeripheralCache.Values.ToList())
                        {
                            if (peripheral.DiscoveryState.State == PeripheralDiscoveryState.Searching)
                            {
                                peripheral.DiscoveryState = (PeripheralDiscoveryState.NotFound, dateFinished);
                            }
                        }
                        observer.OnError(e);
                    }

                    return Disposable.Create(() =>
                    {
                        Logging.Debug($"BLE: Disconnecting from scan observable");
                        cts.Cancel();
                        cts.Dispose();
                        scanSubscription?.Dispose();
                        var dateFinished = DateTimeOffset.UtcNow;
                        foreach (var peripheral in PeripheralCache.Values.ToList())
                        {
                            if (peripheral.DiscoveryState.State == PeripheralDiscoveryState.Searching)
                            {
                                peripheral.DiscoveryState = (PeripheralDiscoveryState.NotFound, dateFinished);
                            }
                        }
                    });
                }
            );
        }

        private BlePeripheral GetPeripheral(Guid peripheralUuid, Guid primaryServiceUuid)
        {
            return PeripheralCache.GetOrAdd(peripheralUuid, _ =>
            {
                var p = (BlePeripheral) Container.Get<IBlePeripheral>();
                p.PeripheralUuid = peripheralUuid;
                p.PrimaryServiceUuid = primaryServiceUuid;
                return p;
            });
        }

        public IDevice GetNativeDeviceFromCache(Guid peripheralUuid)
        {
            return DeviceCache[peripheralUuid];
        }
        
        public async Task<IDevice> GetNativeDevice(Guid peripheralUuid, CancellationToken cancellationToken)
        {
            if (!DeviceCache.TryGetValue(peripheralUuid, out IDevice nativeDevice) || nativeDevice == null)
            {
                await FindErosRadioPeripherals()
                    .FirstAsync(p => p.PeripheralUuid == peripheralUuid).ToTask(cancellationToken);
            }
            return DeviceCache[peripheralUuid];
        }
    }
}
