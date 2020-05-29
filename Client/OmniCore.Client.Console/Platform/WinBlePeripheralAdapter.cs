using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth.Advertisement;
using Nito.AsyncEx;
using OmniCore.Model.Constants;
using OmniCore.Model.Interfaces;
using OmniCore.Model.Interfaces.Services.Internal;

namespace OmniCore.Client.Console.Platform
{
    public class WinBlePeripheralAdapter : IBlePeripheralAdapter
    {
        private readonly IContainer Container;
        private readonly Dictionary<Guid, WinBlePeripheral> PeripheralDictionary;
        private readonly AsyncLock PeripheralDictionaryLock;
        public WinBlePeripheralAdapter(
            IContainer container)
        {
            Container = container;
            PeripheralDictionary = new Dictionary<Guid, WinBlePeripheral>();
            PeripheralDictionaryLock = new AsyncLock();
        }
        
        public async Task<IBlePeripheral> GetPeripheral(Guid peripheralUuid, Guid primaryServiceUuid)
        {
            return await GetPeripheralInternal(peripheralUuid);
        }
        public async Task TryEnsureAdapterEnabled(CancellationToken cancellationToken)
        {
        }

        public async Task<bool> TryEnableAdapter(CancellationToken cancellationToken)
        {
            return true;
        }

        public IObservable<IBlePeripheral> FindErosRadioPeripherals()
        {
            return Observable.Create<IBlePeripheral>(async observer =>
            {
                var watcher = new BluetoothLEAdvertisementWatcher(new BluetoothLEAdvertisementFilter()
                {
                    Advertisement = new BluetoothLEAdvertisement()
                    {
                        ServiceUuids = {Uuids.RileyLinkServiceUuid}
                    }
                });

                watcher.ScanningMode = BluetoothLEScanningMode.Active;
                var discovered = new HashSet<ulong>();
                
                watcher.Received += async (sender, eventArgs) =>
                {
                    if (!discovered.Contains(eventArgs.BluetoothAddress))
                    {
                        var peripheral = await GetPeripheralInternal(eventArgs.BluetoothAddress); 
                        observer.OnNext(peripheral);
                        discovered.Add(eventArgs.BluetoothAddress);
                    }
                };

                watcher.Start();
                return Disposable.Create(() =>
                {
                    watcher.Stop();
                });
            });
        }

        public IObservable<IBlePeripheralAdapter> WhenAdapterDisabled()
        {
            return Observable.Never<IBlePeripheralAdapter>();
        }

        public IObservable<IBlePeripheralAdapter> WhenAdapterEnabled()
        {
            return Observable.Return(this);
        }

        public void InvalidatePeripheralState(IBlePeripheral peripheral)
        {
        }

        private Task<WinBlePeripheral> GetPeripheralInternal(ulong pau)
        {
            return GetPeripheralInternal(ToGuid(pau));
        }
        private async Task<WinBlePeripheral> GetPeripheralInternal(Guid uuid)
        {
            using var _ = await PeripheralDictionaryLock.LockAsync();
            if (!PeripheralDictionary.ContainsKey(uuid))
            {
                var blePeripheral = (WinBlePeripheral) await Container.Get<IBlePeripheral>();
                blePeripheral.PeripheralUuid = uuid;
                blePeripheral.PrimaryServiceUuid = Uuids.RileyLinkServiceUuid;
                PeripheralDictionary[uuid] = blePeripheral;
            }
            return PeripheralDictionary[uuid];
        }
        private static Guid ToGuid(ulong u)
        {
            var b = new byte[16];
            b[15] = (byte) (u       & 0xff);
            b[14] = (byte) (u >>  8 & 0xff);
            b[13] = (byte) (u >> 16 & 0xff);
            b[12] = (byte) (u >> 24 & 0xff);
            b[11] = (byte) (u >> 32 & 0xff);
            b[10] = (byte) (u >> 40 & 0xff);
            return new Guid(b);
        }

        private static ulong ToUlong(Guid g)
        {
            var b = g.ToByteArray();
            return b[15]
                   | (ulong) b[14] << 8
                   | (ulong) b[13] << 16
                   | (ulong) b[12] << 24
                   | (ulong) b[11] << 32
                   | (ulong) b[10] << 40;
        }
    }
}