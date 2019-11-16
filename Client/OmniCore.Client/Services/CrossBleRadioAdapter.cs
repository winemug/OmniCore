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
using OmniCore.Model.Utilities;
using OmniCore.Model.Interfaces;
using Plugin.BluetoothLE;

namespace OmniCore.Client.Services
{
    public class CrossBleRadioAdapter : IRadioAdapter
    {
        private SemaphoreSlim PeripheralAccessSemaphore;
        private ConcurrentDictionary<Guid, CrossBleRadioPeripheral> BlePeripherals;
        public CrossBleRadioAdapter()
        {
            BlePeripherals = new ConcurrentDictionary<Guid, CrossBleRadioPeripheral>();
            PeripheralAccessSemaphore = new SemaphoreSlim(1, 1);
        }

        // do we know the adapter state?
        // can we know the adapter state

        public async Task<bool> BleOperationCanProceed(CancellationToken cancellationToken)
        {
            var stateEval = await EvaluateAdapterState(CrossBleAdapter.Current.Status, cancellationToken);

            if (stateEval.HasValue)
                return stateEval.Value;

            var resultSource = new TaskCompletionSource<bool>();

            try
            {
                CrossBleAdapter.Current.WhenStatusChanged()
                    .Subscribe(async (adapterState) =>
                    {
                        stateEval = await EvaluateAdapterState(adapterState, cancellationToken);
                        if (stateEval.HasValue)
                        {
                            resultSource.TrySetResult(stateEval.Value);
                        }
                    }, cancellationToken
                    );
            }
            catch (TaskCanceledException)
            {
                return false;
            }

            return await resultSource.Task;
        }

        private async Task<bool?> EvaluateAdapterState(AdapterStatus adapterState, CancellationToken cancellationToken)
        {
            switch (adapterState)
            {
                case AdapterStatus.PoweredOn:
                    return true;
                case AdapterStatus.Unsupported:
                    return false;
                case AdapterStatus.Unauthorized:
                case AdapterStatus.PoweredOff:
                    if (CrossBleAdapter.Current.CanControlAdapterState())
                        return await TryEnable(cancellationToken);
                    else
                        return false;
                default:
                    return null;
            }
        }

        public async Task<bool> TryEnable(CancellationToken cancellationToken)
        {
            if (CrossBleAdapter.Current.Status == AdapterStatus.PoweredOn)
            {
                return true;
            }
            try
            {
                var stateTask = CrossBleAdapter.Current.WhenStatusChanged()
                    .Where(s => s == AdapterStatus.PoweredOn).FirstAsync().ToTask(cancellationToken);
                var enableTask = Task.Run(() => { CrossBleAdapter.Current.SetAdapterState(true); }, cancellationToken);
                await Task.WhenAll(stateTask, enableTask);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> TryDisable()
        {
            if (CrossBleAdapter.Current.Status == AdapterStatus.PoweredOff)
                return true;
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

        public IObservable<IRadioPeripheralScanResult> ScanPeripherals(Guid serviceId, CancellationToken cancellationToken)
        {
            return Observable.Create<IRadioPeripheralScanResult>(async (observer) =>
                {
                    if (!await PeripheralAccessSemaphore.WaitAsyncCancellable(cancellationToken))
                    {
                        return Disposable.Empty;
                    }

                    if (!await BleOperationCanProceed(cancellationToken) ||
                        !await DisconnectAllPeripherals(cancellationToken))
                    {
                        return Disposable.Create(() =>
                        {
                            PeripheralAccessSemaphore.Release();
                        });
                    }

                    if (CrossBleAdapter.Current.IsScanning)
                    {
                        CrossBleAdapter.Current.StopScan();
                    }

                    var searchResults = new HashSet<Guid>();
                    var scan = CrossBleAdapter.Current
                        .Scan(new ScanConfig
                        {
                            ScanType = BleScanType.Balanced,
                            ServiceUuids = new List<Guid>() { serviceId },
                            AndroidUseScanBatching = false
                        })
                        .Subscribe((scanResult) =>
                        {
                            UpdatePeripheralDictionary(scanResult.Device);
                            if (!searchResults.Contains(scanResult.Device.Uuid))
                            {
                                searchResults.Add(scanResult.Device.Uuid);
                                var crossScanResult = new CrossBleScanResult
                                {
                                    Id = scanResult.Device.Uuid,
                                    Name = scanResult.Device.Name,
                                    Rssi = scanResult.Rssi
                                };
                                observer.OnNext(crossScanResult);
                            }
                        });
                    return Disposable.Create(() =>
                    {
                        scan.Dispose();
                        if (CrossBleAdapter.Current.IsScanning)
                        {
                            CrossBleAdapter.Current.StopScan();
                        }
                        PeripheralAccessSemaphore.Release();
                    });
                }
            );
        }

        public async Task<IRadioPeripheralLease> LeasePeripheral(Guid id, CancellationToken cancellationToken)
        {
            if (!await PeripheralAccessSemaphore.WaitAsyncCancellable(cancellationToken))
                return null;

            try
            {
                var blePeripheral = await GetPeripheral(id, cancellationToken);
                if (blePeripheral != null)
                {
                    return await blePeripheral.AcquireLease(cancellationToken);
                }
            }
            catch (TaskCanceledException) { }
            finally
            {
                PeripheralAccessSemaphore.Release();
            }
            return null;
        }

        public async Task<List<IRadioPeripheralScanResult>> GetKnownPeripherals(Guid serviceId, CancellationToken cancellationToken)
        {
            //TODO: filter serviceId
            if (!await PeripheralAccessSemaphore.WaitAsyncCancellable(cancellationToken))
                return null;

            try
            {
                return BlePeripherals.Values.ToList()
                    .Select(p => new CrossBleScanResult
                    {
                        Id = p.PeripheralId,
                        Name = p.PeripheralName
                    }).ToList<IRadioPeripheralScanResult>();
            }
            catch { throw; }
            finally
            {
                PeripheralAccessSemaphore.Release();
            }
        }

        private async Task<bool> DisconnectAllPeripherals(CancellationToken cancellationToken)
        {
            try
            {
                var peripheralTasks = new List<Task>();
                foreach (var blePeripheral in BlePeripherals.Values.ToList())
                {
                    peripheralTasks.Add( Task.Run( async () =>
                    {
                        CrossBlePeripheralLease lease = null;
                        try
                        {
                            lease = await blePeripheral.AcquireLease(cancellationToken);
                            await blePeripheral.Disconnect(TimeSpan.Zero);
                        }
                        catch (TaskCanceledException)
                        { }
                        finally
                        {
                            lease?.Dispose();
                            CrossBleRadioPeripheral removedPeripheral = null;
                            BlePeripherals.TryRemove(blePeripheral.PeripheralId, out removedPeripheral);
                        }
                    }, cancellationToken));
                }
                await Task.WhenAll(peripheralTasks);
                return true;
            }
            catch(TaskCanceledException)
            {
                return false;
            }
        }

        private async Task<CrossBleRadioPeripheral> GetPeripheral(Guid peripheralId, CancellationToken cancellationToken)
        {
            CrossBleRadioPeripheral ret = null;
            if (!BlePeripherals.TryGetValue(peripheralId, out ret))
            {
                try
                {
                    var device = (await CrossBleAdapter.Current.GetConnectedDevices().ToTask(cancellationToken)).FirstOrDefault(d => d.Uuid == peripheralId);

                    if (device == null)
                    {
                        device = await CrossBleAdapter.Current.GetKnownDevice(peripheralId).ToTask(TimeSpan.FromSeconds(20), cancellationToken);
                    }

                    if (device == null)
                    {
                        device = await CrossBleAdapter.Current.ScanUntilDeviceFound(peripheralId).ToTask(TimeSpan.FromSeconds(20), cancellationToken);
                    }

                    if (device != null)
                    {
                        return UpdatePeripheralDictionary(device);
                    }
                }
                catch (TaskCanceledException)
                {
                }
            }
            return ret;
        }

        private CrossBleRadioPeripheral UpdatePeripheralDictionary(IDevice newBleDevice)
        {
            var newBlePeripheral = new CrossBleRadioPeripheral(newBleDevice);
            var addedPeripheral = BlePeripherals.AddOrUpdate(newBleDevice.Uuid, newBlePeripheral, (key, oldBlePeripheral) =>
            {
                if (oldBlePeripheral.BleDevice != newBleDevice)
                {
                    oldBlePeripheral.SwitchToNewDevice(newBleDevice);
                }
                return oldBlePeripheral;
            });
            return addedPeripheral;
        }
    }
}
