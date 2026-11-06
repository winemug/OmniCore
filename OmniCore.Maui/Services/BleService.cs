using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.Diagnostics;
using Plugin.BLE.Abstractions;
using Plugin.BLE.Abstractions.Contracts;
using Plugin.BLE.Abstractions.EventArgs;

namespace OmniCore.Maui.Services;

public class BleService
{
    private readonly IBluetoothLE _ble;
    private ConcurrentDictionary<Guid, BleDevice> _trackedDevices;

    public BleService(IBluetoothLE ble)
    {
        _ble = ble;
        _trackedDevices = new ConcurrentDictionary<Guid, BleDevice>();
        _ble.StateChanged += BluetoothOnStateChanged;
        _ble.Adapter.ScanTimeoutElapsed += AdapterOnScanTimeoutElapsed;

        _ble.Adapter.DeviceAdvertised +=(_, e) =>
            GetTrackedDevice(e.Device.Id)?.AddDeviceEvent(BleDeviceEvent.Advertised, e.Device);
        _ble.Adapter.DeviceDiscovered +=(_, e) =>
            GetTrackedDevice(e.Device.Id)?.AddDeviceEvent(BleDeviceEvent.Discovered, e.Device);
        _ble.Adapter.DeviceConnected +=(_, e) =>
            GetTrackedDevice(e.Device.Id)?.AddDeviceEvent(BleDeviceEvent.Connected, e.Device);
        _ble.Adapter.DeviceDisconnected +=(_, e) =>
            GetTrackedDevice(e.Device.Id)?.AddDeviceEvent(BleDeviceEvent.Disconnected, e.Device);
        _ble.Adapter.DeviceConnectionError +=(_, e) =>
            GetTrackedDevice(e.Device.Id)?.AddDeviceEvent(BleDeviceEvent.ConnectionError, e.Device);
        _ble.Adapter.DeviceConnectionLost +=(_, e) =>
            GetTrackedDevice(e.Device.Id)?.AddDeviceEvent(BleDeviceEvent.ConnectionLost, e.Device);
        _ble.Adapter.DeviceBondStateChanged += (_, e) =>
            GetTrackedDevice(e.Device.Id)?.AddDeviceEvent(BleDeviceEvent.BondStateChanged, e.Device); 
    }

    public BleDevice? GetTrackedDevice(Guid deviceId)
    {
        _trackedDevices.TryGetValue(deviceId, out var value);
        return value;
    }
    
    private void BluetoothOnStateChanged(object? sender, BluetoothStateChangedArgs e)
    {
        Debug.WriteLine($"{nameof(BleService)}: Bluetooth state changed from {e.OldState} to {e.NewState}");
    }

    private void AdapterOnScanTimeoutElapsed(object? sender, EventArgs e)
    {
        Debug.WriteLine($"{nameof(BleService)}: Scan timed out, device count: {_trackedDevices.Count}");
    }

    // private static readonly Guid KnownDeviceEma = Guid.Parse("00000000-0000-0000-0000-BC33ACB8F4CA");
    // private static readonly Guid KnownDeviceRlink = Guid.Parse("00000000-0000-0000-0000-886B0FF897CF");
    // private static readonly Guid KnownDeviceOrange = Guid.Parse("00000000-0000-0000-0000-C2C42B149FE4");
    // private static readonly Guid[] KnownDevices =
    // [
    //     KnownDeviceEma,
    //     KnownDeviceRlink,
    //     KnownDeviceOrange
    // ];
    
    private static readonly Guid RadioLinkServiceUuid = Guid.Parse("0235733B99C54197B85669219C2A3845");
    
    public void StartScanning()
    {
        Debug.WriteLine($"Starting scan");
        MainThread.BeginInvokeOnMainThread(async void () =>
        {
            try
            {
                if (_ble.IsAvailable && _ble.IsOn)
                {
                    if (_ble.Adapter.IsScanning)
                    {
                        await _ble.Adapter.StopScanningForDevicesAsync();
                    }
                    _ble.Adapter.ScanMode = ScanMode.Balanced;
                    _ble.Adapter.ScanTimeout = 30000;
                    _ble.Adapter.ScanMatchMode = ScanMatchMode.AGRESSIVE;
                    var scanFilterOptions = new ScanFilterOptions
                    {
                        ServiceUuids = [RadioLinkServiceUuid]
                    };
                    await _ble.Adapter.StartScanningForDevicesAsync(scanFilterOptions);
                }
            }
            catch (Exception e)
            {
                throw; // TODO handle exception
            }
        });
    }
}