using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Linq;
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

        private readonly AsyncLock LeaseLock;
        private IDisposable RssiUpdateSubscription = null;
        private IDisposable ConnectionStateSubscription = null;

        public CrossBleRadioPeripheral(IDevice bleDevice)
        {
            BleDevice = bleDevice;
            LeaseLock = new AsyncLock();
            ConnectionStateSubscription = BleDevice.WhenStatusChanged().Subscribe(
                (connectionStatus) =>
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

        public Guid PeripheralUuid => BleDevice.Uuid;
        public string PeripheralName => BleDevice.Name;

        public async Task<IRadioPeripheralLease> Lease(CancellationToken cancellationToken)
        {
            var leaseLock = await LeaseLock.LockAsync(cancellationToken);
            return new CrossBlePeripheralLease(BleDevice, leaseLock);
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

        public async void Dispose()
        {
            await LeaseLock.LockAsync(CancellationToken.None);
            RssiUpdateSubscription?.Dispose();
            ConnectionStateSubscription.Dispose();
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
