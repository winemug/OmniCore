using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Linq;
using System.Reactive;
using System.Reactive.Subjects;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Nito.AsyncEx;
using OmniCore.Model.Utilities;
using OmniCore.Client.Platform;
using OmniCore.Model.Enumerations;
using OmniCore.Model.Exceptions;
using OmniCore.Model.Interfaces.Common;
using OmniCore.Model.Interfaces.Services;
using OmniCore.Model.Interfaces.Services.Internal;
using OmniCore.Model.Utilities.Extensions;
using Plugin.BluetoothLE;
using Xamarin.Forms;
using Xamarin.Forms.Internals;
using ValueTuple = System.ValueTuple;

namespace OmniCore.Client.Platform
{
    public class BlePeripheral : IBlePeripheral
    {
        public Guid PeripheralUuid { get; set; }
        public Guid PrimaryServiceUuid { get; set; }

        private string _Name;
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

        private (int Rssi, DateTimeOffset Date)? _Rssi = null;
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

        private (PeripheralDiscoveryState,DateTimeOffset) _DiscoveryState =
            (PeripheralDiscoveryState.Unknown, DateTimeOffset.UtcNow);
        
        public (PeripheralDiscoveryState State, DateTimeOffset Date) DiscoveryState
        {
            get => _DiscoveryState;
            set
            {
                _DiscoveryState = value;
                DiscoveryStateSubject.OnNext(value.State);
            }
        }

        private (PeripheralConnectionState State, DateTimeOffset Date) _ConnectionState =
            (PeripheralConnectionState.Disconnected, DateTimeOffset.UtcNow);
        
        public (PeripheralConnectionState State, DateTimeOffset Date) ConnectionState
        {
            get => _ConnectionState;
            set
            {
                _ConnectionState = value;
                ConnectionStateSubject.OnNext(value.State);
            }
        }

        public IObservable<string> WhenNameUpdated() => NameSubject.AsObservable();
        public IObservable<int> WhenRssiReceived() => RssiSubject.AsObservable();
        public async Task<int> ReadRssi(CancellationToken cancellationToken)
        {
            using var pcc = await BlePeripheralAdapter.PeripheralConnectionLock(cancellationToken);
            {
                var device = BlePeripheralAdapter.GetNativeDeviceFromCache(PeripheralUuid);
                if (device == null)
                {
                    throw new OmniCorePeripheralException(FailureType.PeripheralOffline);
                }
                return await device.ReadRssi().FirstAsync().ToTask(cancellationToken);
            }
        }

        public async Task Discover(CancellationToken cancellationToken)
        {
            await BlePeripheralAdapter.FindErosRadioPeripherals()
                .FirstAsync(p => p.PeripheralUuid == PeripheralUuid).ToTask(cancellationToken);
        }

        public IObservable<PeripheralDiscoveryState> WhenDiscoveryStateChanged()
            => DiscoveryStateSubject.AsObservable();
        public IObservable<PeripheralConnectionState> WhenConnectionStateChanged()
            => ConnectionStateSubject.AsObservable();

        private readonly BlePeripheralAdapter BlePeripheralAdapter;
        private readonly ICoreLoggingFunctions Logging;
        private readonly ICoreContainer<IServerResolvable> Container;
        private readonly ICoreApplicationFunctions ApplicationFunctions;
        private readonly AsyncLock PeripheralCommunicationLockProvider;
        private readonly ISubject<PeripheralDiscoveryState> DiscoveryStateSubject;
        private readonly ISubject<PeripheralConnectionState> ConnectionStateSubject;
        private readonly ISubject<string> NameSubject;
        private readonly ISubject<int> RssiSubject;
        
        private IDisposable DeviceStateSubscription;
        private IDisposable DeviceNameSubscription;


        public BlePeripheral(
            IBlePeripheralAdapter blePeripheralAdapter,
            ICoreLoggingFunctions loggingFunctions,
            ICoreApplicationFunctions applicationFunctions,
            ICoreContainer<IServerResolvable> container)
        {
            Container = container;
            BlePeripheralAdapter = (BlePeripheralAdapter) blePeripheralAdapter;
            Logging = loggingFunctions;
            ApplicationFunctions = applicationFunctions;
            PeripheralCommunicationLockProvider = new AsyncLock();
            DiscoveryStateSubject = new BehaviorSubject<PeripheralDiscoveryState>(PeripheralDiscoveryState.Unknown);
            ConnectionStateSubject = new BehaviorSubject<PeripheralConnectionState>(PeripheralConnectionState.Disconnected);
            NameSubject = new Subject<string>();
            RssiSubject = new Subject<int>();
        }

        private IDevice _LastDevice = null;
        public void UpdateSubscriptions(IDevice device)
        {
            if (_LastDevice != null && device != null
                                    && ReferenceEquals(_LastDevice, device))
            {
                return;
            }

            _LastDevice = device;
            
            DeviceStateSubscription?.Dispose();
            DeviceNameSubscription?.Dispose();
            
            DeviceStateSubscription = null;
            DeviceNameSubscription = null;

            if (device == null)
                return;

            DeviceStateSubscription = device.WhenStatusChanged().Subscribe(async status =>
            {
                switch (status)
                {
                    case ConnectionStatus.Disconnected:
                        Logging.Debug($"BLEP: {PeripheralUuid.AsMacAddress()} Device state changed to disconnected");
                        ConnectionStateSubject.OnNext(PeripheralConnectionState.Disconnected);
                        break;
                    case ConnectionStatus.Disconnecting:
                        Logging.Debug($"BLEP: {PeripheralUuid.AsMacAddress()} Device state changed to disconnecting");
                        ConnectionStateSubject.OnNext(PeripheralConnectionState.Disconnecting);
                        break;
                    case ConnectionStatus.Connected:
                        Logging.Debug($"BLEP: {PeripheralUuid.AsMacAddress()} Device state changed to connected");
                        ConnectionStateSubject.OnNext(PeripheralConnectionState.Connected);
                        break;
                    case ConnectionStatus.Connecting:
                        Logging.Debug($"BLEP: {PeripheralUuid.AsMacAddress()} Device state changed to connecting");
                        ConnectionStateSubject.OnNext(PeripheralConnectionState.Connecting);
                        break;
                }
            });
        
            DeviceNameSubscription = device.WhenNameUpdated().Where(s => !string.IsNullOrEmpty(s))
                .Subscribe(s =>
                {
                    Logging.Debug($"BLEP: {PeripheralUuid.AsMacAddress()} Device name updated");
                    Name = s;
                });
        }
        
        public async Task<IBlePeripheralConnection> GetConnection(
            bool autoConnect,
            bool stayConnected,
            TimeSpan discoveryTimeout,
            TimeSpan connectTimeout,
            TimeSpan characteristicDiscoveryTimeout,
            CancellationToken cancellationToken)
        {
            IDevice device = null;

            var lockDisposable = await PeripheralCommunicationLockProvider.LockAsync(cancellationToken);
            await BlePeripheralAdapter.TryEnsureAdapterEnabled(cancellationToken);
            var bluetoothLock = ApplicationFunctions.BluetoothKeepAwake();

            try
            {
                using var pcc = await BlePeripheralAdapter.PeripheralConnectionLock(cancellationToken);
                {
                    using var discoveryTimeoutSource = new CancellationTokenSource(discoveryTimeout);
                    using var discoveryCancellationSource = CancellationTokenSource.CreateLinkedTokenSource(
                        discoveryTimeoutSource.Token,
                        cancellationToken);
                    
                    device = await BlePeripheralAdapter.GetNativeDevice(PeripheralUuid,
                        discoveryCancellationSource.Token);

                    await device.WhenStatusChanged().FirstAsync(s => s == ConnectionStatus.Connected
                                                                     || s == ConnectionStatus.Disconnected)
                        .ToTask(cancellationToken);

                    if (!device.IsConnected())
                    {
                        using var connectTimeoutSource = new CancellationTokenSource(connectTimeout);
                        using var connectCancellationSource = CancellationTokenSource.CreateLinkedTokenSource(
                            connectTimeoutSource.Token,
                            cancellationToken);
                    
                        var exceptionTask = device.WhenConnectionFailed().FirstAsync().ToTask(connectCancellationSource.Token);
                        var connectionTask = device.WhenConnected().FirstAsync().ToTask(connectCancellationSource.Token);
                        
                        device.Connect(new ConnectionConfig()
                            {AndroidConnectionPriority = ConnectionPriority.High, AutoConnect = autoConnect});

                        var which = await Task.WhenAny(exceptionTask, connectionTask);
                        if (which.IsCanceled)
                            throw new OperationCanceledException();
                        
                        if (which == exceptionTask)
                        {
                            Logging.Debug($"BLEP: {PeripheralUuid.AsMacAddress()} Connect failed. Err:\n {exceptionTask.Result.AsDebugFriendly()}");
                            throw exceptionTask.Result;
                        }
                    }
                    
                    using var characteristicTimeoutSource = new CancellationTokenSource(characteristicDiscoveryTimeout);
                    using var characteristicCancellationSource = CancellationTokenSource.CreateLinkedTokenSource(
                        characteristicTimeoutSource.Token,
                        cancellationToken);
                    
                    var characteristicsDictionary = new Dictionary<(Guid ServiceUuid, Guid CharacteristicUuid), IGattCharacteristic>();
                    Logging.Debug($"BLEP: {PeripheralUuid.AsMacAddress()} Request services and characteristics discovery");
                    await device.DiscoverServices().ForEachAsync(service =>
                    {
                        service.DiscoverCharacteristics().ForEachAsync(characteristic =>
                        {
                            characteristicsDictionary.Add((service.Uuid, characteristic.Uuid), characteristic);
                        }, characteristicCancellationSource.Token);
                    }, characteristicCancellationSource.Token);
                    Logging.Debug($"BLEP: {PeripheralUuid.AsMacAddress()} Services and characteristics discovery finished");

                    var blepc = (BlePeripheralConnection) Container.Get<IBlePeripheralConnection>();
                    var communicationDisposable = Disposable.Create(() =>
                    {
                        bluetoothLock.Dispose();
                        lockDisposable.Dispose();
                    });

                    blepc.Initialize(device, characteristicsDictionary, communicationDisposable, stayConnected);
                    return blepc;
                }
            }
            catch (Exception)
            {
                device?.CancelConnection();
                bluetoothLock?.Dispose();
                lockDisposable?.Dispose();
                throw;
            }
        }

        // private void UpdateRssiSubscription()
        // {
        //     DeviceRssiSubscription?.Dispose();
        //     DeviceRssiSubscription = null;
        //     if (InternalDevice != null && RssiAutoUpdateInterval != null)
        //     {
        //         Logging.Debug($"BLEP: {PeripheralUuid.AsMacAddress()} Set rssi read interval");
        //         DeviceRssiSubscription = InternalDevice.WhenReadRssiContinuously(RssiAutoUpdateInterval)
        //             .Subscribe(rssi =>
        //             {
        //                 Logging.Debug($"BLEP: {PeripheralUuid.AsMacAddress()} Rssi received via interval read");
        //                 RssiReceivedSubject.OnNext(rssi);
        //             });
        //     }
        // }

        // private void SetDeviceInternal(IDevice newDevice)
        // {
        //     if (!ReferenceEquals(InternalDevice, newDevice))
        //     {
        //         DeviceStateSubscription?.Dispose();
        //         DeviceNameSubscription?.Dispose();
        //         DeviceRssiSubscription?.Dispose();
        //
        //         DeviceStateSubscription = null;
        //         DeviceNameSubscription = null;
        //         DeviceRssiSubscription = null;
        //
        //         InternalDevice = newDevice;
        //        
        //         if (InternalDevice != null)
        //         {
        //             if (InternalDevice.IsConnected())
        //                 ConnectionStateSubject.OnNext(PeripheralConnectionState.Connected);
        //             
        //             DeviceStateSubscription = InternalDevice.WhenStatusChanged().Subscribe(async status =>
        //             {
        //                 switch (status)
        //                 {
        //                     case ConnectionStatus.Disconnected:
        //                         Logging.Debug($"BLEP: {PeripheralUuid.AsMacAddress()} Device state changed to disconnected");
        //                         ConnectionStateSubject.OnNext(PeripheralConnectionState.Disconnected);
        //                         break;
        //                     case ConnectionStatus.Disconnecting:
        //                         Logging.Debug($"BLEP: {PeripheralUuid.AsMacAddress()} Device state changed to disconnecting");
        //                         ConnectionStateSubject.OnNext(PeripheralConnectionState.Disconnecting);
        //                         break;
        //                     case ConnectionStatus.Connected:
        //                         Logging.Debug($"BLEP: {PeripheralUuid.AsMacAddress()} Device state changed to connected");
        //                         await DiscoverServicesAndCharacteristics(CancellationToken.None);
        //                         ConnectionStateSubject.OnNext(PeripheralConnectionState.Connected);
        //                         break;
        //                     case ConnectionStatus.Connecting:
        //                         Logging.Debug($"BLEP: {PeripheralUuid.AsMacAddress()} Device state changed to connecting");
        //                         ConnectionStateSubject.OnNext(PeripheralConnectionState.Connecting);
        //                         break;
        //                 }
        //             });
        //
        //             DeviceNameSubscription = InternalDevice.WhenNameUpdated().Where(s => !string.IsNullOrEmpty(s))
        //                 .Subscribe(s =>
        //                 {
        //                     Logging.Debug($"BLEP: {PeripheralUuid.AsMacAddress()} Device name updated");
        //                     NameSubject.OnNext(s);
        //                 });
        //
        //             UpdateRssiSubscription();
        //         }
        //     }
        // }



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
        //
        // public async Task<ILease<IBlePeripheral>> Lease(CancellationToken cancellationToken)
        // {
        //     return await Lease<IBlePeripheral>.NewLease(this, cancellationToken);
        // }
        //
        // public bool OnLease { get; set; }
        // public void ThrowIfNotOnLease()
        // {
        //     if (!OnLease)
        //         throw new OmniCoreWorkflowException(FailureType.Internal, "Instance must be leased to perform the operation");
        // }
    }
}
