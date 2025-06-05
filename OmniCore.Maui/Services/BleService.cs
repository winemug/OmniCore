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
    private readonly IAdapter _adapter;

    public BleService()
    {
        _ble = CrossBluetoothLE.Current;
        _adapter = CrossBluetoothLE.Current.Adapter;
        _ble.StateChanged += BluetoothOnStateChanged;
        _adapter.DeviceConnected += AdapterOnDeviceConnected;
        _adapter.DeviceAdvertised += AdapterOnDeviceAdvertised;
        _adapter.DeviceDiscovered += AdapterOnDeviceDiscovered;
        _adapter.DeviceDisconnected += AdapterOnDeviceDisconnected;
        _adapter.ScanTimeoutElapsed += AdapterOnScanTimeoutElapsed;
    }

    private void AdapterOnScanTimeoutElapsed(object? sender, EventArgs e)
    {
        Debug.WriteLine($"{nameof(BleService)}: OnScanTimeoutElapsed");
    }

    private void AdapterOnDeviceDisconnected(object? sender, DeviceEventArgs e)
    {
        Debug.WriteLine($"{nameof(BleService)}: OnDeviceDisconnected");
    }

    private void AdapterOnDeviceDiscovered(object? sender, DeviceEventArgs e)
    {
        Debug.WriteLine($"{nameof(BleService)}: OnDeviceDiscovered");
    }

    private void AdapterOnDeviceAdvertised(object? sender, DeviceEventArgs e)
    {
        Debug.WriteLine($"{nameof(BleService)}: OnDeviceAdvertised");
    }

    private void AdapterOnDeviceConnected(object? sender, DeviceEventArgs e)
    {
        Debug.WriteLine($"{nameof(BleService)}: OnDeviceConnected");
    }

    private void BluetoothOnStateChanged(object? sender, BluetoothStateChangedArgs e)
    {
        Debug.WriteLine($"{nameof(BleService)}: Bluetooth state changed from {e.OldState} to {e.NewState}");
    }

    public async Task Search()
    {
        _adapter.ScanMode = ScanMode.LowLatency;
        _adapter.ScanTimeout = 10000;
        _adapter.ScanMatchMode = ScanMatchMode.STICKY;
        Debug.WriteLine("Start scan");
        await _adapter.StartScanningForDevicesAsync();
        Debug.WriteLine("Started");
    }
}