using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.Diagnostics;
using Nito.AsyncEx;
using OmniCore.Client.Interfaces.Services;
using Plugin.BLE.Abstractions.Contracts;
using Plugin.BLE.Abstractions.EventArgs;

namespace OmniCore.Client.Mobile.Services;

public class BleService : IBleService
{
    private readonly AsyncLazy<IBluetoothLE> _ble;
    private readonly IServiceProvider _serviceProvider;
    private readonly IServiceScopeFactory _serviceScopeFactory;

    private readonly AsyncLock _bleScannerLock = new ();
    private readonly ConcurrentDictionary<Guid, IBleDevice> _bleDeviceDictionary = new ConcurrentDictionary<Guid, IBleDevice>();

    private IServiceScope _deviceTrackingScope;

    public ObservableCollection<IBleDevice> BleDeviceList { get; init; }

    public BleService(
        AsyncLazy<IBluetoothLE> ble,
        IServiceProvider serviceProvider,
        IServiceScopeFactory serviceScopeFactory)
    {
        _ble = ble;
        _serviceProvider = serviceProvider;
        _serviceScopeFactory = serviceScopeFactory;
        _deviceTrackingScope = _serviceScopeFactory.CreateScope();
        BleDeviceList = new ObservableCollection<IBleDevice>();
    }

    private async Task<IAdapter> GetAdapterAsync()
    {
        var ble = await _ble;
        var adapter = ble.Adapter;
        adapter.DeviceDiscovered += AdapterOnDeviceDiscovered;
        adapter.DeviceAdvertised += AdapterOnDeviceAdvertised;
        adapter.ScanTimeoutElapsed += AdapterOnScanTimeoutElapsed;
        ble.StateChanged += BleOnStateChanged;
        return adapter;
    }

    private void BleOnStateChanged(object? sender, BluetoothStateChangedArgs e)
    {
        Debug.WriteLine($"Ble state changed: {e.OldState} {e.NewState}");
    }

    private void AdapterOnScanTimeoutElapsed(object? sender, EventArgs e)
    {
        if (_timer != null)
        {
            _timer.Dispose();
            _timer = null;
        }
        Debug.WriteLine("Scan timed out.");
    }

    private void AdapterOnDeviceAdvertised(object? sender, DeviceEventArgs e)
    {
        //Debug.WriteLine($"Device advertised: {e.Device.Id}");
        AddUpdateBleDevice(e.Device);
    }

    private void AdapterOnDeviceDiscovered(object? sender, DeviceEventArgs e)
    {
        //Debug.WriteLine($"Device discovered: {e.Device.Id}");
        AddUpdateBleDevice(e.Device);
    }

    private void AddUpdateBleDevice(IDevice dev)
    {
        var t = DateTimeOffset.UtcNow;
        var newDevice = false;
        var bleDevice = _bleDeviceDictionary.GetOrAdd(dev.Id,
            guid =>
            {
                var bd = _deviceTrackingScope.ServiceProvider.GetRequiredService<IBleDevice>();
                bd.Address = dev.Id;
                newDevice = true;
                return bd;
            });
        MainThread.InvokeOnMainThreadAsync(() =>
        {
            if (newDevice)
            {
                BleDeviceList.Add(bleDevice);
            }
            if (!string.IsNullOrEmpty(dev.Name))
                bleDevice.Name = dev.Name;
            bleDevice.LastRssi = dev.Rssi;
            bleDevice.LastSeen = t;
        });
    }

    private Timer? _timer;

    public async Task StartSearchAsync(CancellationToken cancellationToken)
    {
        var adapter = await GetAdapterAsync();
        if (adapter.IsScanning)
            return;

        BleDeviceList.Clear();
        _bleDeviceDictionary.Clear();

        _deviceTrackingScope.Dispose();
        _deviceTrackingScope = _serviceScopeFactory.CreateScope();

        _timer = new Timer(_ => DisplayDevices(), null, TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(5));
        adapter.ScanMatchMode = ScanMatchMode.AGRESSIVE;
        adapter.ScanMode = ScanMode.LowLatency;
        adapter.ScanTimeout = 300000;
        adapter.StartScanningForDevicesAsync(cancellationToken: cancellationToken);
    }

    public async Task StopSearchAsync(CancellationToken cancellationToken)
    {
        var adapter = await GetAdapterAsync();
        if (adapter.IsScanning)
        {
            await adapter.StopScanningForDevicesAsync();
            if (_timer != null)
            {
                await _timer.DisposeAsync();
                _timer = null;
            }
        }
    }

    private void DisplayDevices()
    {
        Debug.WriteLine("FOUND DEVICES:");
        foreach (var bleDevice in _bleDeviceDictionary.Values) // .OrderByDescending(bd => bd.LastSeen)
        {
            Debug.WriteLine(bleDevice.ToString());
        }
    }

}