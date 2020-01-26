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
using OmniCore.Model.Extensions;
using OmniCore.Model.Utilities;
using OmniCore.Model.Interfaces.Platform.Common;
using OmniCore.Model.Interfaces.Services;
using Plugin.BluetoothLE;
using Xamarin.Forms.Internals;

namespace OmniCore.Client.Platform
{
    public class RadioAdapter : IRadioAdapter
    {
        private AsyncLock AdapterManagementLock;
        private ConcurrentDictionary<Guid, IRadioPeripheral> PeripheralCache;
        private IDisposable BluetoothWakeLockDisposable;
        private IObservable<IScanResult> ConnectableScanObservable;

        private readonly ICoreContainer<IServerResolvable> Container;
        private readonly ICoreNotificationFunctions NotificationFunctions;
        private readonly ICoreApplicationFunctions ApplicationFunctions;
        private readonly ISubject<IRadioAdapter> ScanStartingSubject;
        private readonly ISubject<IRadioAdapter> ScanFinishedSubject;

        public RadioAdapter(ICoreContainer<IServerResolvable> container,
            ICoreApplicationFunctions applicationFunctions,
            ICoreNotificationFunctions notificationFunctions)
        {
            Container = container;
            ApplicationFunctions = applicationFunctions;
            NotificationFunctions = notificationFunctions;
            AdapterManagementLock = new AsyncLock();
            PeripheralCache = new ConcurrentDictionary<Guid, IRadioPeripheral>();
            ScanStartingSubject = new Subject<IRadioAdapter>();
            ScanFinishedSubject = new Subject<IRadioAdapter>();
            ResetConnectableObservable();
        }

        private void ResetConnectableObservable()
        {
            ConnectableScanObservable = Observable.Create<IScanResult>(observer =>
                {
                    BluetoothWakeLockDisposable = ApplicationFunctions.BluetoothKeepAwake();
                    ScanStartingSubject.OnNext(this);
                    observer.OnCompleted();
                    return Disposable.Empty;
                }).Concat(CrossBleAdapter.Current
                    .Scan())
                .Finally(() =>
                {
                    ScanFinishedSubject.OnNext(this);
                    BluetoothWakeLockDisposable.Dispose();
                    ResetConnectableObservable();
                })
                .Publish()
                .RefCount(TimeSpan.FromSeconds(5));
        }

        public IObservable<IRadioAdapter> WhenDiscoveryStarting() => ScanStartingSubject.AsObservable();
        public IObservable<IRadioAdapter> WhenDiscoveryFinished() => ScanFinishedSubject.AsObservable();
        public IObservable<IRadioAdapter> WhenAdapterDisabled()
        {
            return Observable.Create<IRadioAdapter>(observer =>
            {
                
                if (CrossBleAdapter.Current.Status == AdapterStatus.PoweredOff)
                {
                    observer.OnNext(this);                    
                }

                return CrossBleAdapter.Current.WhenStatusChanged()
                    .Where(s => s == AdapterStatus.PoweredOff)
                    .Subscribe(_ => { observer.OnNext(this); });
            });
        }

        public IObservable<IRadioAdapter> WhenAdapterEnabled()
        {
            return Observable.Create<IRadioAdapter>(observer =>
            {
                if (CrossBleAdapter.Current.Status == AdapterStatus.PoweredOn)
                {
                    observer.OnNext(this);                    
                }

                return CrossBleAdapter.Current.WhenStatusChanged()
                    .Where(s => s == AdapterStatus.PoweredOn)
                    .Subscribe(_ => { observer.OnNext(this); });
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

        public IObservable<IRadioPeripheral> FindPeripherals()
        {
            return Observable.Create<IRadioPeripheral>(async (observer) =>
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

                        var connectedDevices = await CrossBleAdapter.Current
                            .GetConnectedDevices().ToTask(cancellationToken);

                        foreach (var connectedDevice in connectedDevices)
                        {
                            cancellationToken.ThrowIfCancellationRequested();
                            var peripheral = GetPeripheral(connectedDevice.Uuid);
                            using (var _ = peripheral.Lease(cancellationToken))
                            {
                                ((RadioPeripheral) peripheral).SetDevice(connectedDevice);
                            }

                            observer.OnNext(peripheral);
                            observedPeripheralUuids.Add(peripheral.PeripheralUuid);
                        }

                        if (!CrossBleAdapter.Current.IsScanning)
                        {
                            // breathe before starting an implicit scan
                            await Task.Delay(1000, cancellationToken);
                        }

                        scanSubscription = ConnectableScanObservable
                            .Subscribe(async (scanResult) =>
                            {
                                var peripheral = GetPeripheral(scanResult.Device.Uuid) as RadioPeripheral;
                                using (var _ = await peripheral.Lease(cancellationToken))
                                {
                                    peripheral.SetParametersFromScanResult(scanResult);
                                }

                                if (!observedPeripheralUuids.Contains(peripheral.PeripheralUuid))
                                {
                                    observer.OnNext(peripheral);
                                    observedPeripheralUuids.Add(peripheral.PeripheralUuid);
                                }
                            });
                    }
                    catch (Exception e)
                    {
                        scanSubscription?.Dispose();
                        bluetoothLock?.Dispose();
                        observer.OnError(e);
                    }

                    return Disposable.Create(() =>
                    {
                        cts.Cancel();
                        cts.Dispose();
                        scanSubscription?.Dispose();
                        bluetoothLock?.Dispose();
                    });
                }
            );
        }

        public IRadioPeripheral GetPeripheral(Guid peripheralUuid)
        {
            return PeripheralCache.GetOrAdd(peripheralUuid, _ =>
            {
                return Container.Get<IRadioPeripheral>();
            });
        }
    }
}
