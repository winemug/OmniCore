﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Reactive.Threading.Tasks;
using System.Threading;
using System.Threading.Tasks;
using Nito.AsyncEx;
using OmniCore.Model.Entities;
using OmniCore.Model.Enumerations;
using OmniCore.Model.Exceptions;
using OmniCore.Model.Interfaces;
using OmniCore.Model.Interfaces.Services;
using OmniCore.Model.Interfaces.Services.Internal;
using OmniCore.Model.Utilities.Extensions;
using Plugin.BluetoothLE;

namespace OmniCore.Client.Platform
{
    public class BlePeripheral : IBlePeripheral
    {
        private readonly IPlatformFunctions PlatformFunctions;

        private readonly BlePeripheralAdapter BlePeripheralAdapter;
        private readonly ISubject<PeripheralConnectionState> ConnectionStateSubject;
        private readonly IContainer Container;
        private readonly ISubject<PeripheralDiscoveryState> DiscoveryStateSubject;
        private readonly ILogger Logger;
        private readonly ISubject<string> NameSubject;
        private readonly AsyncLock PeripheralCommunicationLockProvider;
        private readonly ISubject<int> RssiSubject;

        private (PeripheralConnectionState State, DateTimeOffset Date) _ConnectionState =
            (PeripheralConnectionState.NotConnected, DateTimeOffset.UtcNow);

        private (PeripheralDiscoveryState State, DateTimeOffset Date) _DiscoveryState =
            (PeripheralDiscoveryState.Unknown, DateTimeOffset.UtcNow);

        private IDevice _LastDevice;

        private string _Name;

        private (int Rssi, DateTimeOffset Date)? _Rssi;
        private IDisposable DeviceNameSubscription;

        private IDisposable DeviceStateSubscription;


        public BlePeripheral(
            IBlePeripheralAdapter blePeripheralAdapter,
            ILogger logger,
            IPlatformFunctions platformFunctions,
            IContainer container)
        {
            Container = container;
            BlePeripheralAdapter = (BlePeripheralAdapter) blePeripheralAdapter;
            Logger = logger;
            PlatformFunctions = platformFunctions;
            PeripheralCommunicationLockProvider = new AsyncLock();
            DiscoveryStateSubject = new BehaviorSubject<PeripheralDiscoveryState>(PeripheralDiscoveryState.Unknown);
            ConnectionStateSubject =
                new BehaviorSubject<PeripheralConnectionState>(PeripheralConnectionState.NotConnected);
            NameSubject = new Subject<string>();
            RssiSubject = new Subject<int>();
        }

        public Guid PeripheralUuid { get; set; }
        public Guid PrimaryServiceUuid { get; set; }

        public string Name
        {
            get => _Name;
            set
            {
                if (string.IsNullOrEmpty(value) && value != _Name)
                {
                    _Name = value;
                    NameSubject.OnNext(value);
                }
            }
        }

        public (int Rssi, DateTimeOffset Date)? Rssi
        {
            get => _Rssi;
            set
            {
                if (value.HasValue)
                {
                    _Rssi = value;
                    RssiSubject.OnNext(value.Value.Rssi);
                }
            }
        }

        public (PeripheralDiscoveryState State, DateTimeOffset Date) DiscoveryState
        {
            get => _DiscoveryState;
            set
            {
                if (value.State != _DiscoveryState.State)
                {
                    Logger.Debug($"BLEP: {PeripheralUuid.AsMacAddress()} Discovery state changed to {value.State}");
                    _DiscoveryState = value;
                    DiscoveryStateSubject.OnNext(value.State);
                }
            }
        }

        public (PeripheralConnectionState State, DateTimeOffset Date) ConnectionState
        {
            get => _ConnectionState;
            set
            {
                if (value.State != _ConnectionState.State && (value.State != PeripheralConnectionState.Disconnected || _ConnectionState.State != PeripheralConnectionState.NotConnected))
                {
                    Logger.Debug($"BLEP: {PeripheralUuid.AsMacAddress()} Connection state changed to {value.State}");
                    _ConnectionState = value;
                    ConnectionStateSubject.OnNext(value.State);
                }
            }
        }

        public IObservable<string> WhenNameUpdated()
        {
            return NameSubject.AsObservable();
        }

        public IObservable<int> WhenRssiReceived()
        {
            return RssiSubject.AsObservable();
        }

        public async Task<int> ReadRssi(CancellationToken cancellationToken)
        {
            var device = BlePeripheralAdapter.GetNativeDeviceFromCache(PeripheralUuid);
            if (device == null) throw new OmniCorePeripheralException(FailureType.PeripheralOffline);
            try
            {
                return await device.ReadRssi().FirstAsync().ToTask(cancellationToken);
            }
            catch (Exception e)
            {
                BlePeripheralAdapter.InvalidatePeripheralState(this);                
                throw;
            }
        }

        public async Task Discover(CancellationToken cancellationToken)
        {
            await BlePeripheralAdapter.FindErosRadioPeripherals()
                .FirstAsync(p => p.PeripheralUuid == PeripheralUuid).ToTask(cancellationToken);
        }

        public IObservable<PeripheralDiscoveryState> WhenDiscoveryStateChanged()
        {
            return DiscoveryStateSubject.AsObservable();
        }

        public IObservable<PeripheralConnectionState> WhenConnectionStateChanged()
        {
            return ConnectionStateSubject.AsObservable();
        }

        public async Task<IBlePeripheralConnection> GetConnection(
            BleOptions options,
            CancellationToken cancellationToken)
        {
            IDevice device = null;

            var lockDisposable = await PeripheralCommunicationLockProvider.LockAsync(cancellationToken);
            await BlePeripheralAdapter.TryEnsureAdapterEnabled(cancellationToken);
            var bluetoothLock = PlatformFunctions.BluetoothLock();

            try
            {
                using (var pcc = await BlePeripheralAdapter.PeripheralConnectionLock(cancellationToken))
                {
                    device = BlePeripheralAdapter.GetNativeDeviceFromCache(PeripheralUuid);
                    if (device == null)
                    {
                        await BlePeripheralAdapter.FindErosRadioPeripherals()
                            .FirstAsync(p => p.PeripheralUuid == PeripheralUuid)
                            .Timeout(options.PeripheralDiscoveryTimeout)
                            .ToTask(cancellationToken);
                    }

                    await device.WhenStatusChanged().FirstAsync(s => s == ConnectionStatus.Connected
                                                                     || s == ConnectionStatus.Disconnected)
                        .Timeout(options.PeripheralConnectTimeout)
                        .ToTask(cancellationToken);

                    if (!device.IsConnected())
                    {
                        var exceptionTask = device.WhenConnectionFailed().FirstAsync()
                            .ToTask(cancellationToken);
                        
                        var connectionTask =
                            device.WhenConnected().FirstAsync()
                                .Timeout(options.PeripheralConnectTimeout)
                                .ToTask(cancellationToken);

                        device.Connect(new ConnectionConfig
                            {AndroidConnectionPriority = ConnectionPriority.High, AutoConnect = options.PeripheralAutoConnect});

                        var which = await Task.WhenAny(exceptionTask, connectionTask);
                        if (which == exceptionTask)
                        {
                            Logger.Debug(
                                $"BLEP: {PeripheralUuid.AsMacAddress()} Connect failed. Err:\n {exceptionTask.Result.AsDebugFriendly()}");
                            throw exceptionTask.Result;
                        }
                        
                        Logger.Debug(
                            $"BLEP: {PeripheralUuid.AsMacAddress()} Requesting MTU of 512 bytes");
                        var mtu = await device.RequestMtu(512).ToTask(cancellationToken);
                        Logger.Debug(
                            $"BLEP: {PeripheralUuid.AsMacAddress()} MTU set at {mtu} bytes");

                    }

                    var characteristicsDictionary =
                        new Dictionary<(Guid ServiceUuid, Guid CharacteristicUuid), IGattCharacteristic>();
                    Logger.Debug(
                        $"BLEP: {PeripheralUuid.AsMacAddress()} Request services and characteristics discovery");

                    await device.DiscoverServices()
                        .Timeout(options.ServicesDiscoveryTimeout)
                        .ForEachAsync(
                        service =>
                        {
                            service.DiscoverCharacteristics()
                                .Timeout(options.CharacteristicsDiscoveryTimeout)
                                .ForEachAsync(
                                characteristic =>
                                {
                                    characteristicsDictionary.Add((service.Uuid, characteristic.Uuid), characteristic);
                                }, cancellationToken);
                        }, cancellationToken);
                    
                    Logger.Debug(
                        $"BLEP: {PeripheralUuid.AsMacAddress()} Services and characteristics discovery finished");

                    //TODO: multiple disposable
                    var communicationDisposable = Disposable.Create(() =>
                    {
                        bluetoothLock.Dispose();
                        lockDisposable.Dispose();
                    });

                    var blepc = (BlePeripheralConnection) await Container.Get<IBlePeripheralConnection>();
                    blepc.Initialize(device, characteristicsDictionary, communicationDisposable, this, options.PeripheralAutoConnect);
                    return blepc;
                }
            }
            catch (Exception)
            {
                device?.CancelConnection();
                bluetoothLock?.Dispose();
                lockDisposable?.Dispose();
                BlePeripheralAdapter.InvalidatePeripheralState(this);
                throw;
            }
        }

        public void UpdateSubscriptions(IDevice device)
        {
            if (_LastDevice != null && device != null
                                    && ReferenceEquals(_LastDevice, device))
                return;

            if (_LastDevice == null && device == null)
                return;
            
            _LastDevice = device;

            DeviceStateSubscription?.Dispose();
            DeviceNameSubscription?.Dispose();

            DeviceStateSubscription = null;
            DeviceNameSubscription = null;

            var now = DateTimeOffset.UtcNow;
            if (device == null)
            {
                ConnectionState = (PeripheralConnectionState.NotConnected, now);
                DiscoveryState = (PeripheralDiscoveryState.Unknown, now);
                return;
            }

            DeviceStateSubscription = device.WhenStatusChanged().Subscribe(status =>
            {
                switch (status)
                {
                    case ConnectionStatus.Connecting:
                        ConnectionState = (PeripheralConnectionState.Connecting, now);
                        break;
                    case ConnectionStatus.Connected:
                        ConnectionState = (PeripheralConnectionState.Connected, now);
                        break;
                    case ConnectionStatus.Disconnecting:
                        ConnectionState = (PeripheralConnectionState.Disconnecting, now);
                        break;
                    case ConnectionStatus.Disconnected:
                        ConnectionState = (PeripheralConnectionState.Disconnected, now);
                        break;
                }
            });

            DeviceNameSubscription = device.WhenNameUpdated().Where(s => !string.IsNullOrEmpty(s))
                .Subscribe(s =>
                {
                    Logger.Debug($"BLEP: {PeripheralUuid.AsMacAddress()} Device name updated");
                    Name = s;
                });
        }
        
        public Task<int> RequestMtu(int size, CancellationToken cancellationToken)
        {
            var device = BlePeripheralAdapter.GetNativeDeviceFromCache(PeripheralUuid);
            if (device == null) throw new OmniCorePeripheralException(FailureType.PeripheralOffline);
            try
            {
                return device.RequestMtu(size).ToTask(cancellationToken);
            }
            catch (Exception e)
            {
                BlePeripheralAdapter.InvalidatePeripheralState(this);
                throw;
            }
        }
    }
}