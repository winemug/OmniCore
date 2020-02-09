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
        private ConcurrentDictionary<Guid, IBlePeripheral> PeripheralCache;
        private IDisposable BluetoothWakeLockDisposable;
        private IObservable<IScanResult> ConnectableScanObservable;

        private readonly ICoreContainer<IServerResolvable> Container;
        private readonly ICoreNotificationFunctions NotificationFunctions;
        private readonly ICoreLoggingFunctions Logging;
        private readonly ICoreApplicationFunctions ApplicationFunctions;
        private readonly IErosRadioProvider[] ErosRadioProviders;
        private readonly ISubject<IBlePeripheralAdapter> ScanStartingSubject;
        private readonly ISubject<IBlePeripheralAdapter> ScanFinishedSubject;
        private readonly ISubject<bool> ScanningStateSubject;
        private readonly List<Guid> ErosRadioServiceUuids;

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
            PeripheralCache = new ConcurrentDictionary<Guid, IBlePeripheral>();
            ScanStartingSubject = new Subject<IBlePeripheralAdapter>();
            ScanFinishedSubject = new Subject<IBlePeripheralAdapter>();
            ScanningStateSubject = new BehaviorSubject<bool>(false);
            
            CrossBleAdapter.AndroidConfiguration.ShouldInvokeOnMainThread =
                CrossBleAdapter.AndroidConfiguration.IsMainThreadSuggested;
            CrossBleAdapter.AndroidConfiguration.UseInternalSyncQueue = true;
            CrossBleAdapter.AndroidConfiguration.UseNewScanner = true;
            CrossBleAdapter.AndroidConfiguration.RefreshServices = true;

            ErosRadioServiceUuids = erosRadioProviders
                .Select(rp => rp.ServiceUuid).ToList();

            ResetConnectableObservable();
        }

        private void ResetConnectableObservable()
        {
            ConnectableScanObservable = Observable.Create<IScanResult>(observer =>
                {
                    Logging.Debug($"BLE: Scan starting");
                    BluetoothWakeLockDisposable = ApplicationFunctions.BluetoothKeepAwake();
                    ScanStartingSubject.OnNext(this);
                    observer.OnCompleted();
                    ScanningStateSubject.OnNext(true);
                    return Disposable.Empty;
                }).Concat(CrossBleAdapter.Current
                    .Scan(new ScanConfig
                            {
                                ScanType = BleScanType.LowLatency,
                                AndroidUseScanBatching = false,
                                ServiceUuids = ErosRadioServiceUuids
                            }))
                .Finally(() =>
                {
                    ResetConnectableObservable();
                    Logging.Debug($"BLE: Scan finished");
                    ScanFinishedSubject.OnNext(this);
                    BluetoothWakeLockDisposable.Dispose();
                    ScanningStateSubject.OnNext(false);
                })
                .Publish()
                .RefCount();
        }

        public IObservable<IBlePeripheralAdapter> WhenDiscoveryStarting() => ScanStartingSubject.AsObservable();
        public IObservable<IBlePeripheralAdapter> WhenDiscoveryFinished() => ScanFinishedSubject.AsObservable();
        public IObservable<bool> IsScanning => ScanningStateSubject.AsObservable();

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

        public async Task<bool> TryDisableAdapter(CancellationToken cancellationToken)
        {
            using var adapterManagementLock = await AdapterManagementLock.LockAsync(cancellationToken);
            
            if (CrossBleAdapter.Current.Status == AdapterStatus.PoweredOff)
                return true;
            
            Logging.Debug($"BLE: Trying to disable adapter");
            CrossBleAdapter.Current.SetAdapterState(false);
            Logging.Debug($"BLE: Waiting for adapter to get disabled");
            await CrossBleAdapter.Current.WhenStatusChanged()
                .Where(s => s == AdapterStatus.PoweredOff)
                .FirstAsync()
                .ToTask(cancellationToken);

            Logging.Debug($"BLE: Adapter disabled successfully");
            return CrossBleAdapter.Current.Status == AdapterStatus.PoweredOff;
        }

        public IObservable<IBlePeripheral> FindErosRadioPeripherals()
        {
            return Observable.Create<IBlePeripheral>(async (observer) =>
                {
                    var cts = new CancellationTokenSource();
                    var cancellationToken = cts.Token;

                    var observedPeripheralUuids = new HashSet<Guid>();

                    IDisposable bluetoothLock = null;
                    IDisposable scanSubscription = null;

                    try
                    {
                        await TryEnsureAdapterEnabled(cancellationToken);
                        cancellationToken.ThrowIfCancellationRequested();

                        bluetoothLock = ApplicationFunctions.BluetoothKeepAwake();

                        Logging.Debug($"BLE: Request connected devices");
                        var connectedDevices = await CrossBleAdapter.Current
                            .GetConnectedDevices().ToTask(cancellationToken);
                        Logging.Debug($"BLE: Received connected devices");

                        foreach (var connectedDevice in connectedDevices)
                        {
                            var service= await connectedDevice.DiscoverServices()
                                .FirstOrDefaultAsync(s => ErosRadioServiceUuids.Contains(s.Uuid));

                            if (service != null)
                            {
                                var peripheral = GetPeripheral(connectedDevice.Uuid, service.Uuid);
                                ((BlePeripheral) peripheral).SetDevice(connectedDevice);

                                Logging.Debug(
                                    $"BLE: {peripheral.PeripheralUuid.AsMacAddress()} Notifying connected peripheral as found");
                                observer.OnNext(peripheral);
                                observedPeripheralUuids.Add(peripheral.PeripheralUuid);
                            }
                            cancellationToken.ThrowIfCancellationRequested();
                        }

                        Logging.Debug($"BLE: Connecting to scan observable");
                        scanSubscription = ConnectableScanObservable
                            .Subscribe(async (scanResult) =>
                            {
                                var peripheral = GetPeripheral(scanResult.Device.Uuid,
                                    scanResult.AdvertisementData.ServiceUuids[0]) as BlePeripheral;
                                
                                peripheral.SetParametersFromScanResult(scanResult);

                                if (!observedPeripheralUuids.Contains(peripheral.PeripheralUuid))
                                {
                                    Logging.Debug($"BLE: {peripheral.PeripheralUuid.AsMacAddress()} Notifying found peripheral");
                                    observer.OnNext(peripheral);
                                    observedPeripheralUuids.Add(peripheral.PeripheralUuid);
                                }
                            });
                    }
                    catch (Exception e)
                    {
                        Logging.Debug($"BLE: Error during scan: \n {e.AsDebugFriendly()}");
                        cts.Cancel();
                        cts.Dispose();
                        scanSubscription?.Dispose();
                        bluetoothLock?.Dispose();
                        observer.OnError(e);
                    }

                    return Disposable.Create(() =>
                    {
                        Logging.Debug($"BLE: Disconnecting from scan observable");
                        cts.Cancel();
                        cts.Dispose();
                        scanSubscription?.Dispose();
                        bluetoothLock?.Dispose();
                    });
                }
            );
        }

        public IBlePeripheral GetPeripheral(Guid peripheralUuid, Guid primaryServiceUuid)
        {
            return PeripheralCache.GetOrAdd(peripheralUuid, _ =>
            {
                var p = (BlePeripheral) Container.Get<IBlePeripheral>();
                p.PeripheralUuid = peripheralUuid;
                p.PrimaryServiceUuid = primaryServiceUuid;
                return p;
            });
        }
    }
}
