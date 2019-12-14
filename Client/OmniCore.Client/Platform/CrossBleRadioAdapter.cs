using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
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
using OmniCore.Model.Interfaces.Platform;
using OmniCore.Model.Interfaces.Services;
using Unity;

namespace OmniCore.Client.Platform
{
    public class CrossBleRadioAdapter : IRadioAdapter
    {
        private AsyncLock PeripheralSearchLock;
        private AsyncLock AdapterManagementLock;

        [Dependency]
        private IApplicationService ApplicationService { get; set; }
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
            var stateTask = CrossBleAdapter.Current.WhenStatusChanged()
                .Where(s => s == AdapterStatus.PoweredOn)
                .FirstAsync()
                .ToTask(cancellationToken);
                
            var enableTask = Task.Run(() => CrossBleAdapter.Current.SetAdapterState(true), cancellationToken);
            await Task.WhenAll(stateTask, enableTask);
            return CrossBleAdapter.Current.Status == AdapterStatus.PoweredOn;
        }

        public async Task<bool> TryDisableAdapter(CancellationToken cancellationToken)
        {
            using var adapterManagementLock = await AdapterManagementLock.LockAsync(cancellationToken);
            
            if (CrossBleAdapter.Current.Status == AdapterStatus.PoweredOff)
                return true;
            
            var stateTask = CrossBleAdapter.Current.WhenStatusChanged()
                .Where(s => s == AdapterStatus.PoweredOff)
                .FirstAsync()
                .ToTask(cancellationToken);
                
            var disableTask = Task.Run(() => CrossBleAdapter.Current.SetAdapterState(false), cancellationToken);
            await Task.WhenAll(stateTask, disableTask);
            return CrossBleAdapter.Current.Status == AdapterStatus.PoweredOff;
        }

        public IObservable<IRadioPeripheralResult> FindPeripherals(Guid serviceId)
        {
            return Observable.Create<IRadioPeripheralResult>(async (observer) =>
                {
                    var cts = new CancellationTokenSource();
                    var cancellationToken = cts.Token;
                    
                    try
                    {
                        await TryEnsureAdapterEnabled(cancellationToken);
                        using var searchLock = await PeripheralSearchLock.LockAsync(cancellationToken);

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
                                    Peripheral = new CrossBleRadioPeripheral(connectedDevice),
                                    Rssi = await connectedDevice.ReadRssi().ToTask(cancellationToken)
                                };

                                searchResultDictionary.Add(connectedDevice.Uuid, connectedResult);
                                observer.OnNext(connectedResult);
                            }
                        }

                        if (CrossBleAdapter.Current.IsScanning)
                            CrossBleAdapter.Current.StopScan();

                        using var scanSubscription = CrossBleAdapter.Current
                            .Scan(new ScanConfig
                            {
                                ScanType = BleScanType.Balanced,
                                ServiceUuids = new List<Guid>() {serviceId},
                                AndroidUseScanBatching = false
                            })
                            .Subscribe((scanResult) =>
                            {
                                if (searchResultDictionary.ContainsKey(scanResult.Device.Uuid))
                                {
                                    searchResultDictionary[scanResult.Device.Uuid]
                                        .Rssi = scanResult.Rssi;
                                }
                                else
                                {
                                    var crossResult = new CrossBleResult
                                    {
                                        Peripheral = new CrossBleRadioPeripheral(scanResult.Device),
                                        Rssi = scanResult.Rssi
                                    };
                                    searchResultDictionary.Add(scanResult.Device.Uuid, crossResult);
                                    observer.OnNext(crossResult);
                                }
                            });
                    }
                    catch (Exception e)
                    {
                        observer.OnError(e);
                    }
                    finally
                    {
                        observer.OnCompleted();
                        cts.Dispose();
                    }

                    return Disposable.Create(() =>
                    {
                        cts.Cancel();
                        cts.Dispose();
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
