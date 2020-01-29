using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Nito.AsyncEx;
using OmniCore.Model.Utilities;
using OmniCore.Client.Platform;
using OmniCore.Model.Enumerations;
using OmniCore.Model.Exceptions;
using OmniCore.Model.Extensions;
using OmniCore.Model.Interfaces.Platform.Common;
using Plugin.BluetoothLE;
using Xamarin.Forms.Internals;
using ValueTuple = System.ValueTuple;

namespace OmniCore.Client.Platform
{
    public class RadioPeripheral : IRadioPeripheral
    {
        private readonly IRadioAdapter RadioAdapter;
        private readonly ICoreContainer<IServerResolvable> CoreContainer;

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

        public Guid? PeripheralUuid => Device?.Uuid;
        public Guid[] ServiceUuids { get; private set; }
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

        public RadioPeripheral(IRadioAdapter radioAdapter,
            ICoreContainer<IServerResolvable> coreContainer)
        {
            RadioAdapter = radioAdapter;
            CoreContainer = coreContainer;

            NameSubject = new ParticularBehaviorSubject<string>(null);
            StateSubject = new ParticularBehaviorSubject<PeripheralState>(PeripheralState.Offline);
            ConnectionStateSubject = new ParticularBehaviorSubject<PeripheralConnectionState>(PeripheralConnectionState.Disconnected);
            RssiReceivedSubject = new Subject<int>();

            radioAdapter.WhenDiscoveryStarting().Subscribe( _ =>
            {
                if (Device != null && Device.IsConnected())
                    return;

                SetDeviceInternal(null);
                StateSubject.OnNext(Model.Enumerations.PeripheralState.Discovering);
            });

            radioAdapter.WhenDiscoveryFinished().Subscribe(_ =>
            {
                if (Device == null)
                    StateSubject.OnNext(Model.Enumerations.PeripheralState.Offline);
            });
        }

        public void RequestRssi()
        {
            ThrowIfNotOnLease();

            Device?.ReadRssi().Subscribe(rssi => RssiReceivedSubject.OnNext(rssi));
        }

        public async Task Locate(CancellationToken cancellationToken)
        {
            if (await State.FirstAsync() == PeripheralState.Offline)
            {
                await RadioAdapter
                    .FindPeripherals()
                    .FirstAsync(p => p.PeripheralUuid == PeripheralUuid)
                    .ToTask(cancellationToken);
            }

            await State.FirstAsync(s => s == PeripheralState.Online || s == PeripheralState.Busy).ToTask(cancellationToken);
        }

        public async Task Connect(bool autoConnect, CancellationToken cancellationToken)
        {
            ThrowIfNotOnLease();
            await Locate(cancellationToken);
            var state = await ConnectionState.FirstAsync();
            if (state == PeripheralConnectionState.Connected)
                return;

            if (state != PeripheralConnectionState.Connecting)
            {
                await ConnectionState.FirstAsync(s => s == PeripheralConnectionState.Disconnected).ToTask(cancellationToken);
                Device.Connect(new ConnectionConfig()
                    {AndroidConnectionPriority = ConnectionPriority.Normal, AutoConnect = autoConnect});
            }

            var connectedTask = ConnectionState.FirstAsync(s => s == PeripheralConnectionState.Connected).ToTask(cancellationToken);
            var failedTask = Device.WhenConnectionFailed().ToTask(cancellationToken);

            var which = await Task.WhenAny(connectedTask, failedTask);
            if (which == failedTask)
                throw new OmniCorePeripheralException(FailureType.ConnectionFailed, null, failedTask.Result);
        }

        public async Task Disconnect(CancellationToken cancellationToken)
        {
            ThrowIfNotOnLease();

            switch (await State.FirstAsync())
            {
                case Model.Enumerations.PeripheralState.Offline:
                case Model.Enumerations.PeripheralState.Discovering:
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
                    Device?.CancelConnection();
                    break;
            }

            await ConnectionState.FirstAsync(s => s == PeripheralConnectionState.Disconnected).ToTask(cancellationToken);
        }

        public async Task<byte[]> ReadFromCharacteristic(Guid serviceUuid, Guid characteristicUuid, CancellationToken cancellationToken)
        {
            ThrowIfNotOnLease();

            return (await GetCharacteristic(serviceUuid, characteristicUuid).Read().ToTask(cancellationToken)).Data;
        }

        public async Task WriteToCharacteristic(Guid serviceUuid, Guid characteristicUuid, byte[] data, CancellationToken cancellationToken)
        {
            ThrowIfNotOnLease();

            await GetCharacteristic(serviceUuid, characteristicUuid).Write(data).ToTask(cancellationToken);
        }

        public async Task WriteToCharacteristicWithoutResponse(Guid serviceUuid, Guid characteristicUuid, byte[] data,
            CancellationToken cancellationToken)
        {
            ThrowIfNotOnLease();

            await GetCharacteristic(serviceUuid, characteristicUuid).WriteWithoutResponse(data).ToTask(cancellationToken);
        }

        public IObservable<byte[]> WhenCharacteristicNotificationReceived(Guid serviceUuid, Guid characteristicUuid)
        {
            ThrowIfNotOnLease();

            return GetCharacteristic(serviceUuid, characteristicUuid)
                .RegisterAndNotify()
                .Select(r => r.Data);
        }

        public void SetDevice(IDevice newDevice)
        {
            ThrowIfNotOnLease();

            SetDeviceInternal(newDevice);
        }

        public void SetParametersFromScanResult(IScanResult scanResult)
        {
            ThrowIfNotOnLease();

            SetDeviceInternal(scanResult.Device);
            ServiceUuids = scanResult.AdvertisementData.ServiceUuids;

            if (!string.IsNullOrEmpty(scanResult.AdvertisementData.LocalName))
            {
                NameSubject.OnNext(scanResult.AdvertisementData.LocalName);
            }

            RssiReceivedSubject.OnNext(scanResult.Rssi);
        }

        private void UpdateRssiSubscription()
        {
            DeviceRssiSubscription?.Dispose();
            DeviceRssiSubscription = null;
            if (Device != null && RssiAutoUpdateInterval != null)
            {
                DeviceRssiSubscription = Device.WhenReadRssiContinuously(RssiAutoUpdateInterval)
                    .Subscribe(rssi => RssiReceivedSubject.OnNext(rssi));
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

                var oldDevice = Device;
                Device = newDevice;
                
                if (oldDevice != null && newDevice == null)
                {
                    StateSubject.OnNext(PeripheralState.Offline);
                }
                
                if (Device != null)
                {
                    StateSubject.OnNext(PeripheralState.Online);
                    DeviceStateSubscription = Device.WhenStatusChanged().Subscribe(async status =>
                    {
                        switch (status)
                        {
                            case ConnectionStatus.Disconnected:
                                ConnectionStateSubject.OnNext(PeripheralConnectionState.Disconnected);
                                break;
                            case ConnectionStatus.Disconnecting:
                                ConnectionStateSubject.OnNext(PeripheralConnectionState.Disconnecting);
                                break;
                            case ConnectionStatus.Connected:
                                await DiscoverServicesAndCharacteristics(CancellationToken.None);
                                ConnectionStateSubject.OnNext(PeripheralConnectionState.Connected);
                                break;
                            case ConnectionStatus.Connecting:
                                ConnectionStateSubject.OnNext(PeripheralConnectionState.Connecting);
                                break;
                        }
                    });

                    DeviceNameSubscription = Device.WhenNameUpdated().Where(s => !string.IsNullOrEmpty(s))
                        .Subscribe(s => NameSubject.OnNext(s));

                    UpdateRssiSubscription();
                }
            }
        }

        private async Task DiscoverServicesAndCharacteristics(CancellationToken cancellationToken)
        {
            CharacteristicsDictionary = new Dictionary<(Guid ServiceUuid, Guid CharacteristicUuid), IGattCharacteristic>();
            await Device.DiscoverServices().ForEachAsync(service =>
            {
                service.DiscoverCharacteristics().ForEachAsync(characteristic =>
                {
                    CharacteristicsDictionary.Add((service.Uuid, characteristic.Uuid), characteristic);
                }, cancellationToken);
            }, cancellationToken);
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

        public async Task<ILease<IRadioPeripheral>> Lease(CancellationToken cancellationToken)
        {
            return await Lease<IRadioPeripheral>.NewLease(this, cancellationToken);
        }

        public bool OnLease { get; set; }
        public void ThrowIfNotOnLease()
        {
            if (!OnLease)
                throw new OmniCoreWorkflowException(FailureType.Internal, "Instance must be leased to perform the operation");
        }
    }
}
