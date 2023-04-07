using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Nito.AsyncEx;
using OmniCore.Services.Interfaces;
using OmniCore.Services.Interfaces.Core;
using OmniCore.Services.Interfaces.Radio;
using Plugin.BLE;
using Plugin.BLE.Abstractions.Contracts;
using Plugin.BLE.Abstractions.EventArgs;

namespace OmniCore.Services;

public class RadioService : IRadioService
{
    private Dictionary<Guid, AsyncLock> _radioLocks;
    private Dictionary<Guid, IDevice?> _radioDevices;
    private List<Radio> _radios;

    public RadioService()
    {
        _radioLocks = new Dictionary<Guid, AsyncLock>();
        _radioDevices = new Dictionary<Guid, IDevice?>();
        _radios = new List<Radio>();
    }
    public async Task Start()
    {
        var ble = CrossBluetoothLE.Current;
        var adapter = ble.Adapter;
        
        ble.StateChanged += BleOnStateChanged;
        adapter.DeviceDiscovered += AdapterOnDeviceDiscovered;
        adapter.ScanTimeoutElapsed += AdapterOnScanTimeoutElapsed;
        adapter.DeviceDisconnected += AdapterOnDeviceDisconnected;
        
        Debug.WriteLine("starting radios");
        _radios = new List<Radio>
        {
            new(Guid.Parse("00000000-0000-0000-0000-bc33acb95371"), "ema")
            //new Radio(Guid.Parse("00000000-0000-0000-0000-886b0ff897cf"), "mod"),
            //new Radio(Guid.Parse("00000000-0000-0000-0000-c2c42b149fe4"), "ora"),
        };

        foreach(var radio in _radios)
            _radioLocks.Add(radio.Id, new AsyncLock());

        foreach(var radio in _radios)
            _radioDevices.Add(radio.Id, null);
    }

    public async Task Stop()
    {
        Debug.WriteLine("stopping radios");
        foreach (var radio in _radios) radio.Dispose();
        _radios = null;
        var ble = CrossBluetoothLE.Current;
        var adapter = ble.Adapter;
        adapter.DeviceDisconnected -= AdapterOnDeviceDisconnected;
        adapter.ScanTimeoutElapsed -= AdapterOnScanTimeoutElapsed;
        adapter.DeviceDiscovered -= AdapterOnDeviceDiscovered;
        ble.StateChanged -= BleOnStateChanged;
    }

    private void AdapterOnDeviceDisconnected(object? sender, DeviceEventArgs e)
    {
    }

    private void BleOnStateChanged(object? sender, BluetoothStateChangedArgs e)
    {
        if (e.NewState == BluetoothState.On)
        {
        }
    }

    private void AdapterOnScanTimeoutElapsed(object? sender, EventArgs e)
    {
    }

    private void AdapterOnDeviceDiscovered(object? sender, DeviceEventArgs e)
    {
    }

    public async Task<IRadioConnection> GetIdealConnectionAsync(
        CancellationToken cancellationToken = default)
    {
        if (_radios.Count == 0)
            return null;
        if (_radios.Count == 1)
            return await GetConnectionAsync(_radios[0], cancellationToken);

        await Task.WhenAll(_radios.Select(r => r.UpdateRssiAsync(cancellationToken)));

        var radio = _radios.Where(r => r.Rssi.HasValue)
            .OrderByDescending(r => r.Rssi).FirstOrDefault();

        if (radio == null)
            return null;

        return await GetConnectionAsync(radio, cancellationToken);
    }

    public async Task<IRadioConnection> GetConnectionByNameAsync(string name,
        CancellationToken cancellationToken = default)
    {
        var radio = _radios.Where(r => r.Name == name).FirstOrDefault();
        if (radio == null)
            return null;
        return await GetConnectionAsync(radio, cancellationToken);
    }

    public async Task<IRadioConnection> GetConnectionByIdAsync(Guid id,
        CancellationToken cancellationToken = default)
    {
        var radio = _radios.Where(r => r.Id == id).FirstOrDefault();
        if (radio == null)
            return null;
        return await GetConnectionAsync(radio, cancellationToken);
    }

    private async Task<IRadioConnection> GetConnectionAsync(Radio radio, CancellationToken cancellationToken)
    {
        var allocationLockDisposable = await radio.LockAsync(cancellationToken);
        return new RadioConnection(radio, allocationLockDisposable);
    }
}