using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Nito.AsyncEx;
using OmniCore.Model.Utilities;
using OmniCore.Client.Platform;
using OmniCore.Model.Enumerations;
using OmniCore.Model.Exceptions;
using OmniCore.Model.Extensions;
using OmniCore.Model.Interfaces;
using OmniCore.Model.Interfaces.Platform;
using Plugin.BluetoothLE;
using Unity;
using Xamarin.Forms.Internals;

namespace OmniCore.Client.Platform
{
    public class CrossBleRadioPeripheral : IRadioPeripheral
    {
        public IDevice BleDevice { get; private set; }

        private AsyncLock LeaseLock;
        public IDisposable ActiveLeaseLockDisposable { get; private set; }
        private IDisposable RssiUpdateSubscription = null;
        private IDisposable ConnectionStateSubscription = null;
        private IDisposable NameSubscription = null;

        public CrossBleRadioPeripheral(IDevice bleDevice)
        {
            BleDevice = bleDevice;
            LeaseLock = new AsyncLock();
            NameSubscription = BleDevice.WhenNameUpdated().Subscribe((name) => this.Name = name);
            ConnectionStateSubscription = BleDevice.WhenStatusChanged().Subscribe(
                async (connectionStatus) =>
                {
                    ConnectionStateDate = DateTimeOffset.UtcNow;
                    switch (connectionStatus)
                    {
                        case ConnectionStatus.Disconnected:
                            ConnectionState = PeripheralConnectionState.Disconnected;
                            DisconnectDate = ConnectionStateDate;
                            break;
                        case ConnectionStatus.Disconnecting:
                            ConnectionState = PeripheralConnectionState.Disconnecting;
                            DisconnectDate = null;
                            break;
                        case ConnectionStatus.Connected:
                            ConnectionState = PeripheralConnectionState.Connected;
                            DisconnectDate = null;
                            break;
                        case ConnectionStatus.Connecting:
                            ConnectionState = PeripheralConnectionState.Connecting;
                            break;
                    }
                }
            );
        }

        public Guid Uuid => BleDevice.Uuid;
        public string Name { get; set; }

        public async Task<IRadioPeripheralLease> Lease(CancellationToken cancellationToken)
        {
            ActiveLeaseLockDisposable = await LeaseLock.LockAsync(cancellationToken);
            return new CrossBlePeripheralLease(this);
        }

        private TimeSpan? RssiUpdateTimeSpanInternal = null;
        public TimeSpan? RssiUpdateTimeSpan
        {
            get => RssiUpdateTimeSpanInternal;
            set
            {
                if (value == null)
                {
                    RssiUpdateSubscription?.Dispose();
                    RssiUpdateSubscription = null;
                }
                else
                {
                    RssiUpdateSubscription?.Dispose();
                    RssiUpdateSubscription = BleDevice.ReadRssiContinuously(value).Subscribe(rssi =>
                    {
                        Rssi = rssi;
                        RssiDate = DateTimeOffset.UtcNow;
                    });
                }

                RssiUpdateTimeSpanInternal = value;
            }
        }

        private int? rssi;
        public int? Rssi { get => rssi;
            set
            {
                if (value != null)
                {
                    rssi = value;
                    RssiDate = DateTime.UtcNow;
                }
            }
        }
        public DateTimeOffset? RssiDate { get; private set; }
        public PeripheralConnectionState ConnectionState { get; private set; }
        public DateTimeOffset? ConnectionStateDate { get; private set; }
        public DateTimeOffset? DisconnectDate { get; private set; }

        public async Task<int> ReadRssi()
        {
            return await BleDevice.ReadRssi();
        }

        //public async Task<string> ReadName()
        //{
        //    if (BleDevice == null || !BleDevice.IsConnected())
        //        return null;

        //    Guid genericAccessUuid = new Guid(0, 0,0 ,0, 0, 0, 0, 0, 0, 0x18,0);
        //    Guid nameCharacteristicUuid = new Guid(0, 0,0 ,0, 0, 0, 0, 0, 0, 0x2a,0);
        //    var nameCharacteristic = await BleDevice
        //        .GetKnownService(genericAccessUuid)
        //        .SelectMany(x => x.GetKnownCharacteristics(new Guid[] {nameCharacteristicUuid}))
        //        .FirstAsync();

        //    var result = await nameCharacteristic.Read();
        //    return Encoding.ASCII.GetString(result.Data);
        //}

        public void Dispose()
        {
            using var leaseLock = LeaseLock.Lock(CancellationToken.None);
            NameSubscription?.Dispose();
            RssiUpdateSubscription?.Dispose();
            ConnectionStateSubscription.Dispose();
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
