using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.Diagnostics;
using OmniCore.Maui.Constants;
using Plugin.BLE;
using Plugin.BLE.Abstractions;
using Plugin.BLE.Abstractions.Contracts;
using Plugin.BLE.Abstractions.EventArgs;

namespace OmniCore.Maui.Services;

public class BleService
{
    private readonly IBluetoothLE _ble;
    private Dictionary<Guid, BleDevice> _devices;
    public ObservableCollection<BleDevice> BleDevices { get; set; }

    public BleService(IBluetoothLE ble)
    {
        _ble = ble;
        _devices = new Dictionary<Guid, BleDevice>();
        // AddInitialDevices(_ble.Adapter.BondedDevices);
        // AddInitialDevices(_ble.Adapter.ConnectedDevices);
        // AddInitialDevices(_ble.Adapter.DiscoveredDevices);
        BleDevices = new ObservableCollection<BleDevice>(_devices.Values);

        _ble.StateChanged += BluetoothOnStateChanged;
        _ble.Adapter.DeviceConnected += AdapterOnDeviceConnected;
        _ble.Adapter.DeviceAdvertised += AdapterOnDeviceAdvertised;
        _ble.Adapter.DeviceDiscovered += AdapterOnDeviceDiscovered;
        _ble.Adapter.DeviceDisconnected += AdapterOnDeviceDisconnected;
        _ble.Adapter.ScanTimeoutElapsed += AdapterOnScanTimeoutElapsed;

        StartScanning();
    }

    private void AddInitialDevices(IReadOnlyList<IDevice>? devices)
    {
        if (devices == null)
            return;
        foreach (var device in devices)
        {
            if (!_devices.ContainsKey(device.Id))
            {
                var bleDevice = new BleDevice(device);
                _devices.Add(device.Id, bleDevice);
            }
        }
    }

    private void BluetoothOnStateChanged(object? sender, BluetoothStateChangedArgs e)
    {
        Debug.WriteLine($"{nameof(BleService)}: Bluetooth state changed from {e.OldState} to {e.NewState}");
        StartScanning();
    }

    private void AdapterOnScanTimeoutElapsed(object? sender, EventArgs e)
    {
        Debug.WriteLine($"{nameof(BleService)}: Scan timed out, device count: {_devices.Count}");
        MainThread.InvokeOnMainThreadAsync(StartScanning);
    }

    private void AdapterOnDeviceDiscovered(object? sender, DeviceEventArgs e)
    {
        var bleDevice = GetBleDevice(e.Device);
        bleDevice?.AdapterEvent(AdapterDeviceEvent.Discovered, e.Device);
    }

    private void AdapterOnDeviceAdvertised(object? sender, DeviceEventArgs e)
    {
        var bleDevice = GetBleDevice(e.Device);
        bleDevice?.AdapterEvent(AdapterDeviceEvent.Advertised, e.Device);
    }

    private void AdapterOnDeviceConnected(object? sender, DeviceEventArgs e)
    {
        var bleDevice = GetBleDevice(e.Device);
        bleDevice?.AdapterEvent(AdapterDeviceEvent.Connected, e.Device);
    }

    private void AdapterOnDeviceDisconnected(object? sender, DeviceEventArgs e)
    {
        var bleDevice = GetBleDevice(e.Device);
        bleDevice?.AdapterEvent(AdapterDeviceEvent.Disconnected, e.Device);
    }

    private BleDevice? GetBleDevice(IDevice device)
    {
        lock (_devices)
        {
            if (!_devices.TryGetValue(device.Id, out var bleDevice))
            {
                bleDevice = new BleDevice(device);
                _devices.Add(device.Id, bleDevice);
                BleDevices.Add(bleDevice);
            }
            return bleDevice;
        }
    }

    private static readonly Guid KnownDeviceEma = Guid.Parse("00000000-0000-0000-0000-BC33ACB8F4CA");
    private static readonly Guid KnownDeviceRlink = Guid.Parse("00000000-0000-0000-0000-88680FF897CF");
    private static readonly Guid KnownDeviceOrange = Guid.Parse("00000000-0000-0000-0000-BC33ACB8F4CB");

    private static readonly Guid[] KnownDevices = new[]
    {
        KnownDeviceEma,
        KnownDeviceRlink,
        KnownDeviceOrange
    };
    
    public void StartScanning()
    {
        if (_ble.IsAvailable && _ble.IsOn && !_ble.Adapter.IsScanning)
        {
            Debug.WriteLine($"Starting scan");
            _ = MainThread.InvokeOnMainThreadAsync(() =>
            {
                _ble.Adapter.ScanMode = ScanMode.Balanced;
                _ble.Adapter.ScanTimeout = 30000;
                _ble.Adapter.ScanMatchMode = ScanMatchMode.STICKY;
                return _ble.Adapter.StartScanningForDevicesAsync((ScanFilterOptions?)null, device => KnownDevices.Contains(device.Id), true, CancellationToken.None);
            });
        }
    }
}