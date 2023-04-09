using System;
using Nito.AsyncEx;
using OmniCore.Services.Interfaces.Entities;
using OmniCore.Services.Interfaces.Pod;
using OmniCore.Services.Interfaces.Radio;
using Plugin.BLE;
using Plugin.BLE.Abstractions.Contracts;
using Plugin.BLE.Abstractions.EventArgs;

namespace OmniCore.Services;

public class BleService
{
    private readonly IAdapter _adapter;
    private readonly IBluetoothLE _ble;
    private AsyncLock _bleLock = new();

    public BleService()
    {
        _ble = CrossBluetoothLE.Current;
        _adapter = _ble.Adapter;

        _ble.StateChanged += BleOnStateChanged;
        _adapter.DeviceAdvertised += AdapterOnDeviceAdvertised;
        _adapter.DeviceConnected += AdapterOnDeviceConnected;
        _adapter.DeviceDisconnected += AdapterOnDeviceDisconnected;
        _adapter.DeviceDiscovered += AdapterOnDeviceDiscovered;
        _adapter.DeviceConnectionLost += AdapterOnDeviceConnectionLost;
        _adapter.ScanTimeoutElapsed += AdapterOnScanTimeoutElapsed;
    }

    private void AdapterOnScanTimeoutElapsed(object sender, EventArgs e)
    {
    }

    private void AdapterOnDeviceConnectionLost(object sender, DeviceErrorEventArgs e)
    {
    }

    private void AdapterOnDeviceDiscovered(object sender, DeviceEventArgs e)
    {
    }

    private void AdapterOnDeviceDisconnected(object sender, DeviceEventArgs e)
    {
    }

    private void AdapterOnDeviceConnected(object sender, DeviceEventArgs e)
    {
    }

    private void AdapterOnDeviceAdvertised(object sender, DeviceEventArgs e)
    {
    }

    private void BleOnStateChanged(object sender, BluetoothStateChangedArgs e)
    {
    }

    // public async Task<List<BleDevice>> FindDevices(
    //     TimeSpan scanTime,
    //     CancellationToken ct)
    // {
    //     var list = new List<BleDevice>();
    //     using(var _ = await _bleLock.LockAsync(ct))
    //     {
    //         await EnsureBleEnabled(ct);
    //         if _adapter.IsScanning():
    //             await _adapter.StopScanningForDevicesAsync();
    //     }
    // }
    //
    // private async Task EnsureBleEnabled(CancellationToken ct)
    // {
    // }
}