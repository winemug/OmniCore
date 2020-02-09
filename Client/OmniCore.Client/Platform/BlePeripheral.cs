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
using OmniCore.Model.Entities;
using OmniCore.Model.Enumerations;
using OmniCore.Model.Exceptions;
using OmniCore.Model.Extensions;
using OmniCore.Model.Interfaces.Platform.Common;
using Plugin.BluetoothLE;
using Xamarin.Forms.Internals;
using ValueTuple = System.ValueTuple;

namespace OmniCore.Client.Platform
{
    public class BlePeripheral : IBlePeripheral
    {
        private readonly IBlePeripheralAdapter BlePeripheralAdapter;
        private readonly ICoreContainer<IServerResolvable> CoreContainer;
        private readonly ICoreLoggingFunctions Logging;

        private Dictionary<(Guid ServiceUuid, Guid CharacteristicUuid), IGattCharacteristic> CharacteristicsDictionary;
        private IDisposable DeviceStateSubscription;
        private IDisposable DeviceNameSubscription;
        private IDisposable DeviceRssiSubscription;

        private IDevice Device;
        private ISubject<string> NameSubject;
        private ISubject<PeripheralState> StateSubject;
        private ISubject<PeripheralConnectionState> ConnectionStateSubject;
        private ISubject<int> RssiReceivedSubject;
        private TimeSpan? RssiAutoUpdateIntervalInternal;
        public Guid PeripheralUuid { get; set; }
        public Guid PrimaryServiceUuid { get; set; }
        public IObservable<string> Name => NameSubject;
        public IObservable<PeripheralState> State => StateSubject;
        public IObservable<PeripheralConnectionState> ConnectionState => ConnectionStateSubject;
        public IObservable<int> Rssi => RssiReceivedSubject;

        public TimeSpan? RssiAutoUpdateInterval
        {
            get => RssiAutoUpdateIntervalInternal;
            set
            {
                RssiAutoUpdateIntervalInternal = value;
                UpdateRssiSubscription();
            }
        }

        public BlePeripheral(IBlePeripheralAdapter blePeripheralAdapter,
            ICoreContainer<IServerResolvable> coreContainer,
            ICoreLoggingFunctions loggingFunctions)
        {
            BlePeripheralAdapter = blePeripheralAdapter;
            CoreContainer = coreContainer;
            Logging = loggingFunctions;
            
            NameSubject = new ParticularBehaviorSubject<string>(null);
            StateSubject = new ParticularBehaviorSubject<PeripheralState>(PeripheralState.Unknown);
            ConnectionStateSubject = new ParticularBehaviorSubject<PeripheralConnectionState>(PeripheralConnectionState.Disconnected);
            RssiReceivedSubject = new Subject<int>();

            blePeripheralAdapter.WhenDiscoveryStarting().Subscribe( _ =>
            {

                if (Device != null && Device.IsConnected())
                {
                    Logging.Debug($"BLEP: {PeripheralUuid.AsMacAddress()} Device is connected, not resetting internal reference");
                    return;
                }

                Logging.Debug($"BLEP: {PeripheralUuid.AsMacAddress()} Resetting internal reference");
                SetDeviceInternal(null);
                StateSubject.OnNext(PeripheralState.Searching);
            });

            blePeripheralAdapter.WhenDiscoveryFinished().Subscribe(_ =>
            {
                if (Device == null)
                {
                    Logging.Debug($"BLEP: {PeripheralUuid.AsMacAddress()} Discovery finished with no internal reference.");
                    StateSubject.OnNext(Model.Enumerations.PeripheralState.Offline);
                }
                else
                {
                    Logging.Debug($"BLEP: {PeripheralUuid.AsMacAddress()} Discovery finished, internal reference available.");
                    StateSubject.OnNext(Model.Enumerations.PeripheralState.Online);
                }
            });
        }

        public void RequestRssi()
        {
            ThrowIfNotOnLease();

            if (Device != null)
            {
                Logging.Debug($"BLEP: {PeripheralUuid.AsMacAddress()} Rssi requested");
                Device.ReadRssi().Subscribe(rssi =>
                {
                    Logging.Debug($"BLEP: {PeripheralUuid.AsMacAddress()} Rssi received");
                    RssiReceivedSubject.OnNext(rssi);
                });
            }
        }

        
        public IObservable<IBlePeripheral> Locate()
        {
            return BlePeripheralAdapter
                .FindErosRadioPeripherals()
                .FirstAsync(p => p.PeripheralUuid == PeripheralUuid)
                .Concat(State
                    .FirstAsync(s => s == PeripheralState.Online)
                    .Select(s => this));
        }

        public async Task Connect(bool autoConnect, CancellationToken cancellationToken)
        {
            ThrowIfNotOnLease();
            await Locate().ToTask(cancellationToken);
            var state = await ConnectionState.FirstAsync();
            if (state == PeripheralConnectionState.Connected)
                return;

            if (state != PeripheralConnectionState.Connecting)
            {
                await ConnectionState.FirstAsync(s => s == PeripheralConnectionState.Disconnected).ToTask(cancellationToken);
                await BlePeripheralAdapter.IsScanning.FirstAsync(s => !s).ToTask(cancellationToken);
                Logging.Debug($"BLEP: {PeripheralUuid.AsMacAddress()} Connect requested");
                Device.Connect(new ConnectionConfig()
                    {AndroidConnectionPriority = ConnectionPriority.High, AutoConnect = autoConnect});
            }

            var connectedTask = ConnectionState.FirstAsync(s => s == PeripheralConnectionState.Connected).ToTask(cancellationToken);
            var failedTask = Device.WhenConnectionFailed().ToTask(cancellationToken);

            var which = await Task.WhenAny(connectedTask, failedTask);
            if (which == failedTask)
            {
                Logging.Debug($"BLEP: {PeripheralUuid.AsMacAddress()} Connect failed");
                throw new OmniCorePeripheralException(FailureType.ConnectionFailed, null, failedTask.Result);
            }
            Logging.Debug($"BLEP: {PeripheralUuid.AsMacAddress()} Connected");
        }

        public async Task Disconnect(CancellationToken cancellationToken)
        {
            ThrowIfNotOnLease();

            switch (await State.FirstAsync())
            {
                case Model.Enumerations.PeripheralState.Offline:
                case Model.Enumerations.PeripheralState.Searching:
                    return;
            }

            await State.FirstAsync(s => s == Model.Enumerations.PeripheralState.Online).ToTask(cancellationToken);

            switch (await ConnectionState.FirstAsync())
            {
                case PeripheralConnectionState.Disconnecting:
                    break;
                case PeripheralConnectionState.Disconnected:
                    return;
                case PeripheralConnectionState.Connecting:
                case PeripheralConnectionState.Connected:
                    Logging.Debug($"BLEP: {PeripheralUuid.AsMacAddress()} Cancel Connection requested");
                    Device?.CancelConnection();
                    break;
            }

            await ConnectionState.FirstAsync(s => s == PeripheralConnectionState.Disconnected).ToTask(cancellationToken);
            Logging.Debug($"BLEP: {PeripheralUuid.AsMacAddress()} Disconnect confirmed");
        }

        public async Task<byte[]> ReadFromCharacteristic(Guid serviceUuid, Guid characteristicUuid, CancellationToken cancellationToken)
        {
            ThrowIfNotOnLease();

            Logging.Debug($"BLEP: {PeripheralUuid.AsMacAddress()} Read from characteristic requested");
            var result = await GetCharacteristic(serviceUuid, characteristicUuid).Read().ToTask(cancellationToken); 
            Logging.Debug($"BLEP: {PeripheralUuid.AsMacAddress()} Read from characteristic result received");
            return result.Data;
        }

        public async Task WriteToCharacteristic(Guid serviceUuid, Guid characteristicUuid, byte[] data, CancellationToken cancellationToken)
        {
            ThrowIfNotOnLease();

            Logging.Debug($"BLEP: {PeripheralUuid.AsMacAddress()} Write to characteristic requested");
            await GetCharacteristic(serviceUuid, characteristicUuid).Write(data).ToTask(cancellationToken);
            Logging.Debug($"BLEP: {PeripheralUuid.AsMacAddress()} Write to characteristic finished");
        }

        public async Task WriteToCharacteristicWithoutResponse(Guid serviceUuid, Guid characteristicUuid, byte[] data,
            CancellationToken cancellationToken)
        {
            ThrowIfNotOnLease();
            Logging.Debug($"BLEP: {PeripheralUuid.AsMacAddress()} Write to characteristic without response requested");
            await GetCharacteristic(serviceUuid, characteristicUuid).WriteWithoutResponse(data).ToTask(cancellationToken);
            Logging.Debug($"BLEP: {PeripheralUuid.AsMacAddress()} Write to characteristic without response finished");
        }

        public IObservable<byte[]> WhenCharacteristicNotificationReceived(Guid serviceUuid, Guid characteristicUuid)
        {
            ThrowIfNotOnLease();

            Logging.Debug($"BLEP: {PeripheralUuid.AsMacAddress()} Characteristic notification received");
            return GetCharacteristic(serviceUuid, characteristicUuid)
                .RegisterAndNotify()
                .Select(r => r.Data);
        }

        public void SetDevice(IDevice newDevice)
        {
            ThrowIfNotOnLease();

            SetDeviceInternal(newDevice);
            if (newDevice == null)
                StateSubject.OnNext(PeripheralState.Unknown);
            else
                StateSubject.OnNext(PeripheralState.Online);
            Logging.Debug($"BLEP: {PeripheralUuid.AsMacAddress()} Internal reference set from adapter");
        }

        public void SetParametersFromScanResult(IScanResult scanResult)
        {
            ThrowIfNotOnLease();

            SetDeviceInternal(scanResult.Device);
            
            Logging.Debug($"BLEP: {PeripheralUuid.AsMacAddress()} Internal reference set from scan result");
            PrimaryServiceUuid = scanResult.AdvertisementData.ServiceUuids[0];

            if (!string.IsNullOrEmpty(scanResult.AdvertisementData.LocalName))
            {
                NameSubject.OnNext(scanResult.AdvertisementData.LocalName);
            }
            StateSubject.OnNext(PeripheralState.Online);
            RssiReceivedSubject.OnNext(scanResult.Rssi);
        }

        private void UpdateRssiSubscription()
        {
            DeviceRssiSubscription?.Dispose();
            DeviceRssiSubscription = null;
            if (Device != null && RssiAutoUpdateInterval != null)
            {
                Logging.Debug($"BLEP: {PeripheralUuid.AsMacAddress()} Set rssi read interval");
                DeviceRssiSubscription = Device.WhenReadRssiContinuously(RssiAutoUpdateInterval)
                    .Subscribe(rssi =>
                    {
                        Logging.Debug($"BLEP: {PeripheralUuid.AsMacAddress()} Rssi received via interval read");
                        RssiReceivedSubject.OnNext(rssi);
                    });
            }
        }

        private void SetDeviceInternal(IDevice newDevice)
        {
            if (!ReferenceEquals(Device, newDevice))
            {
                DeviceStateSubscription?.Dispose();
                DeviceNameSubscription?.Dispose();
                DeviceRssiSubscription?.Dispose();

                DeviceStateSubscription = null;
                DeviceNameSubscription = null;
                DeviceRssiSubscription = null;

                Device = newDevice;
               
                if (Device != null)
                {
                    if (Device.IsConnected())
                        ConnectionStateSubject.OnNext(PeripheralConnectionState.Connected);
                    
                    DeviceStateSubscription = Device.WhenStatusChanged().Subscribe(async status =>
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
                                await DiscoverServicesAndCharacteristics(CancellationToken.None);
                                ConnectionStateSubject.OnNext(PeripheralConnectionState.Connected);
                                break;
                            case ConnectionStatus.Connecting:
                                Logging.Debug($"BLEP: {PeripheralUuid.AsMacAddress()} Device state changed to connecting");
                                ConnectionStateSubject.OnNext(PeripheralConnectionState.Connecting);
                                break;
                        }
                    });

                    DeviceNameSubscription = Device.WhenNameUpdated().Where(s => !string.IsNullOrEmpty(s))
                        .Subscribe(s =>
                        {
                            Logging.Debug($"BLEP: {PeripheralUuid.AsMacAddress()} Device name updated");
                            NameSubject.OnNext(s);
                        });

                    UpdateRssiSubscription();
                }
            }
        }

        private async Task DiscoverServicesAndCharacteristics(CancellationToken cancellationToken)
        {
            CharacteristicsDictionary = new Dictionary<(Guid ServiceUuid, Guid CharacteristicUuid), IGattCharacteristic>();
            Logging.Debug($"BLEP: {PeripheralUuid.AsMacAddress()} Request services and characteristics discovery");
            await Device.DiscoverServices().ForEachAsync(service =>
            {
                service.DiscoverCharacteristics().ForEachAsync(characteristic =>
                {
                    CharacteristicsDictionary.Add((service.Uuid, characteristic.Uuid), characteristic);
                }, cancellationToken);
            }, cancellationToken);
            Logging.Debug($"BLEP: {PeripheralUuid.AsMacAddress()} Services and characteristics discovery finished");
        }

        private IGattCharacteristic GetCharacteristic(Guid serviceUuid, Guid characteristicUuid)
        {
            var characteristic = CharacteristicsDictionary[(serviceUuid, characteristicUuid)];
            if (characteristic == null)
                throw new OmniCorePeripheralException(FailureType.PeripheralGeneralError, "Characteristic not found on peripheral");
            return characteristic;
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

        public async Task<ILease<IBlePeripheral>> Lease(CancellationToken cancellationToken)
        {
            return await Lease<IBlePeripheral>.NewLease(this, cancellationToken);
        }

        public bool OnLease { get; set; }
        public void ThrowIfNotOnLease()
        {
            if (!OnLease)
                throw new OmniCoreWorkflowException(FailureType.Internal, "Instance must be leased to perform the operation");
        }
    }
}
