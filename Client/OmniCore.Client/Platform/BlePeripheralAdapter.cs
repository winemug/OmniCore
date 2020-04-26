using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Threading;
using System.Threading.Tasks;
using Nito.AsyncEx;
using OmniCore.Model.Enumerations;
using OmniCore.Model.Exceptions;
using OmniCore.Model.Interfaces;
using OmniCore.Model.Interfaces.Services;
using OmniCore.Model.Interfaces.Services.Internal;
using OmniCore.Model.Utilities.Extensions;
using Plugin.BluetoothLE;

namespace OmniCore.Client.Platform
{
    public class BlePeripheralAdapter : IBlePeripheralAdapter
    {
        private readonly IPlatformFunctions PlatformFunctions;

        private readonly IContainer Container;
        private readonly IErosRadioProvider[] ErosRadioProviders;
        private readonly List<Guid> ErosRadioServiceUuids;
        private readonly ILogger Logger;

        private readonly AsyncLock PeripheralConnectionLockProvider;
        private readonly BlePeripheralScanner Scanner;
        private readonly AsyncLock AdapterManagementLock;
        private readonly ConcurrentDictionary<Guid, IDevice> DeviceCache;
        private readonly ConcurrentDictionary<Guid, BlePeripheral> PeripheralCache;

        public BlePeripheralAdapter(IContainer container,
            IPlatformFunctions platformFunctions,
            ILogger logger,
            IErosRadioProvider[] erosRadioProviders)
        {
            Container = container;
            PlatformFunctions = platformFunctions;
            Logger = logger;
            ErosRadioProviders = erosRadioProviders;

            AdapterManagementLock = new AsyncLock();
            PeripheralCache = new ConcurrentDictionary<Guid, BlePeripheral>();
            DeviceCache = new ConcurrentDictionary<Guid, IDevice>();

            CrossBleAdapter.AndroidConfiguration.ShouldInvokeOnMainThread = false;
            CrossBleAdapter.AndroidConfiguration.UseInternalSyncQueue = true;
            CrossBleAdapter.AndroidConfiguration.UseNewScanner = true;
            CrossBleAdapter.AndroidConfiguration.RefreshServices = false;

            ErosRadioServiceUuids = erosRadioProviders
                .Select(rp => rp.ServiceUuid).ToList();

            PeripheralConnectionLockProvider = new AsyncLock();
            Scanner = new BlePeripheralScanner(ErosRadioServiceUuids, logger, platformFunctions);
            WhenScanStarted = Scanner.WhenScanStateChanged.Where(s => s).Select(s => this);
            WhenScanFinished = Scanner.WhenScanStateChanged.Where(s => !s).Select(s => this);

            InternalObservable = CreateObservable();
        }

        public IObservable<IBlePeripheralAdapter> WhenScanStarted { get; }
        public IObservable<IBlePeripheralAdapter> WhenScanFinished { get; }

        public IObservable<IBlePeripheralAdapter> WhenAdapterDisabled()
        {
            return Observable.Create<IBlePeripheralAdapter>(observer =>
            {
                if (CrossBleAdapter.Current.Status == AdapterStatus.PoweredOff)
                {
                    Logger.Debug("BLE: Adapter disabled");
                    observer.OnNext(this);
                }

                return CrossBleAdapter.Current.WhenStatusChanged()
                    .Where(s => s == AdapterStatus.PoweredOff)
                    .Subscribe(_ =>
                    {
                        Logger.Debug("BLE: Adapter disabled");
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
                    Logger.Debug("BLE: Adapter enabled");
                    observer.OnNext(this);
                }

                return CrossBleAdapter.Current.WhenStatusChanged()
                    .Where(s => s == AdapterStatus.PoweredOn)
                    .Subscribe(_ =>
                    {
                        Logger.Debug("BLE: Adapter enabled");
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
                        if (await TryEnableAdapter(cancellationToken))
                            return;
                    throw new OmniCoreAdapterException(FailureType.AdapterNotEnabled);
            }
        }

        public async Task<bool> TryEnableAdapter(CancellationToken cancellationToken)
        {
            using var adapterManagementLock = await AdapterManagementLock.LockAsync(cancellationToken);
            if (CrossBleAdapter.Current.Status == AdapterStatus.PoweredOn) return true;

            Logger.Debug("BLE: Trying to enable adapter");
            CrossBleAdapter.Current.SetAdapterState(true);
            Logger.Debug("BLE: Waiting for adapter to get enabled");

            await CrossBleAdapter.Current.WhenStatusChanged()
                .Where(s => s == AdapterStatus.PoweredOn)
                .FirstAsync()
                .ToTask(cancellationToken);

            Logger.Debug("BLE: Adapter enabled successfully");
            return CrossBleAdapter.Current.Status == AdapterStatus.PoweredOn;
        }

        public async Task<IDisposable> PeripheralConnectionLock(CancellationToken cancellationToken)
        {
            IDisposable bluetoothLock = null;
            IDisposable lockDisposable = null;
            try
            {
                lockDisposable = await PeripheralConnectionLockProvider.LockAsync(cancellationToken);
                cancellationToken.ThrowIfCancellationRequested();

                await TryEnsureAdapterEnabled(cancellationToken);
                cancellationToken.ThrowIfCancellationRequested();

                Scanner.Pause();
                bluetoothLock = PlatformFunctions.BluetoothLock();

                await Task.Delay(500, cancellationToken);
            }
            catch (OperationCanceledException)
            {
                bluetoothLock?.Dispose();
                Scanner.Resume();
                lockDisposable?.Dispose();
            }

            return Disposable.Create(async () =>
            {
                bluetoothLock?.Dispose();
                await Task.Delay(500);
                Scanner.Resume();
                lockDisposable?.Dispose();
            });
        }

        private IObservable<IBlePeripheral> InternalObservable;
        public IObservable<IBlePeripheral> FindErosRadioPeripherals() => InternalObservable;
        private IObservable<IBlePeripheral> CreateObservable()
        {
            return Observable.Create<IBlePeripheral>(async observer =>
                {
                    var cts = new CancellationTokenSource();
                    var cancellationToken = cts.Token;

                    var observedPeripheralUuids = new HashSet<Guid>();

                    IDisposable scanSubscription = null;

                    try
                    {
                        Logger.Debug("BLE: Request connected devices");
                        BlePeripheral connectedPeripheral = null;
                        IEnumerable<IDevice> connectedDevices = new List<IDevice>();
                        using (var ppc = await PeripheralConnectionLock(cts.Token))
                        {
                            try
                            {
                                connectedDevices = await CrossBleAdapter.Current
                                    .GetConnectedDevices().ToTask(cancellationToken);
                                Logger.Debug("BLE: Received connected devices");

                                foreach (var connectedDevice in connectedDevices)
                                {
                                    var service = await connectedDevice.DiscoverServices()
                                        .FirstOrDefaultAsync(s => ErosRadioServiceUuids.Contains(s.Uuid))
                                        .ToTask(cancellationToken);

                                    if (service != null)
                                    {
                                        DeviceCache[connectedDevice.Uuid] = connectedDevice;
                                        connectedPeripheral =
                                            await GetPeripheralInternal(connectedDevice.Uuid, service.Uuid);
                                        connectedPeripheral.UpdateSubscriptions(connectedDevice);

                                        Logger.Debug(
                                            $"BLE: {connectedPeripheral.PeripheralUuid.AsMacAddress()} Notifying connected peripheral as found");
                                        observedPeripheralUuids.Add(connectedPeripheral.PeripheralUuid);
                                        observer.OnNext(connectedPeripheral);
                                    }
                                }
                            }
                            catch (OperationCanceledException)
                            {
                                if (connectedPeripheral != null)
                                    InvalidatePeripheralState(connectedPeripheral);
                            }
                            catch (Exception e)
                            {
                                Logger.Warning("Error occured while retrieving connected devices, continuing with regular scan", e);
                                if (connectedPeripheral != null)
                                    InvalidatePeripheralState(connectedPeripheral);
                            }

                            var searchStart = DateTimeOffset.UtcNow;
                            var connectedPeripheralUuids = connectedDevices.Select(c => c.Uuid);
                            foreach (var peripheralUuid in DeviceCache.Keys.ToList())
                            {
                                if (!connectedPeripheralUuids.Any(cuuid => cuuid == peripheralUuid))
                                    DeviceCache[peripheralUuid] = null;

                                var peripheral = await GetPeripheralInternal(peripheralUuid, ErosRadioServiceUuids[0]);
                                peripheral.DiscoveryState = (PeripheralDiscoveryState.Searching, searchStart);
                            }

                            Logger.Debug("BLE: Connecting to scan observable");
                            scanSubscription = Scanner.Scan()
                                .Subscribe(async scanResult =>
                                {
                                    DeviceCache[scanResult.Device.Uuid] = scanResult.Device;
                                    var peripheral = await GetPeripheralInternal(scanResult.Device.Uuid,
                                        scanResult.AdvertisementData.ServiceUuids[0]);

                                    peripheral.UpdateSubscriptions(scanResult.Device);

                                    if (string.IsNullOrEmpty(peripheral.Name))
                                        peripheral.Name = scanResult.AdvertisementData.LocalName;

                                    if (!string.IsNullOrEmpty(scanResult.Device.Name))
                                        peripheral.Name = scanResult.Device.Name;

                                    peripheral.Rssi = (scanResult.Rssi, DateTimeOffset.UtcNow);
                                    peripheral.DiscoveryState = (PeripheralDiscoveryState.Discovered,
                                        DateTimeOffset.UtcNow);

                                    if (!observedPeripheralUuids.Contains(peripheral.PeripheralUuid))
                                    {
                                        Logger.Debug(
                                            $"BLE: {peripheral.PeripheralUuid.AsMacAddress()} Reporting found peripheral");
                                        observedPeripheralUuids.Add(peripheral.PeripheralUuid);
                                        observer.OnNext(peripheral);
                                    }
                                });
                        }
                    }
                    catch (Exception e)
                    {
                        Logger.Debug($"BLE: Error during scan: \n {e.AsDebugFriendly()}");
                        cts.Cancel();
                        cts.Dispose();
                        scanSubscription?.Dispose();
                        var dateFinished = DateTimeOffset.UtcNow;
                        foreach (var peripheral in PeripheralCache.Values.ToList())
                            if (peripheral.DiscoveryState.State == PeripheralDiscoveryState.Searching)
                                peripheral.DiscoveryState = (PeripheralDiscoveryState.NotFound, dateFinished);
                        observer.OnError(e);
                    }

                    return Disposable.Create(() =>
                    {
                        Logger.Debug("BLE: Disconnecting from scan observable");
                        cts.Cancel();
                        cts.Dispose();
                        scanSubscription?.Dispose();
                        var dateFinished = DateTimeOffset.UtcNow;
                        foreach (var peripheral in PeripheralCache.Values.ToList())
                            if (peripheral.DiscoveryState.State == PeripheralDiscoveryState.Searching)
                                peripheral.DiscoveryState = (PeripheralDiscoveryState.NotFound, dateFinished);
                    });
                }
            );
        }

        public async Task<IBlePeripheral> GetPeripheral(Guid peripheralUuid, Guid primaryServiceUuid)
        {
            return await GetPeripheralInternal(peripheralUuid, primaryServiceUuid);
        }
        
        private async Task<BlePeripheral> GetPeripheralInternal(Guid peripheralUuid, Guid primaryServiceUuid)
        {
            var p = (BlePeripheral) await Container.Get<IBlePeripheral>();
            return PeripheralCache.GetOrAdd(peripheralUuid, _ =>
            {
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
            if (!DeviceCache.TryGetValue(peripheralUuid, out var nativeDevice) || nativeDevice == null)
                await FindErosRadioPeripherals()
                    .FirstAsync(p => p.PeripheralUuid == peripheralUuid).ToTask(cancellationToken);
            return DeviceCache[peripheralUuid];
        }

        public void InvalidatePeripheralState(IBlePeripheral peripheral)
        {
            DeviceCache.TryRemove(peripheral.PeripheralUuid, out _);
            ((BlePeripheral)peripheral).UpdateSubscriptions(null);
        }
    }
}