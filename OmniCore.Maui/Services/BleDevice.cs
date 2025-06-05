using CommunityToolkit.Mvvm.ComponentModel;
using OmniCore.Maui.Models;
using Plugin.BLE.Abstractions;
using Plugin.BLE.Abstractions.Contracts;

namespace OmniCore.Maui.Services;

public partial class BleDevice : ObservableObject
{
    public Guid Id { get; }

    public string FriendlyId =>
        string.Join(':', Id.ToByteArray(true)[new Range(10, 16)] .Select(b => b.ToString("X2")));

    [ObservableProperty]
    public Color _signalColor;


    [ObservableProperty] public string _name;
    [ObservableProperty] public int _rssi;
    public List<BleDeviceRecord> Records { get; }
    
    public BleDevice(IDevice device)
    {
        Id = device.Id;
        Name = device.Name;
        Rssi = device.Rssi;
        var record = new BleDeviceRecord()
        {
            Name = device.Name,
            State = device.State,
            Rssi = device.Rssi,
            AdvertisementRecords = device.AdvertisementRecords,
            SupportsIsConnectable = device.SupportsIsConnectable,
            IsConnectable = device.IsConnectable,
            BondState = device.BondState,
            Time = DateTimeOffset.UtcNow,
            RecordEvent = AdapterDeviceEvent.None,
        };
        Records = new List<BleDeviceRecord>();
        Records.Add(record);
    }

    public void AdapterEvent(AdapterDeviceEvent adapterDeviceEvent, IDevice device)
    {
        var record = new BleDeviceRecord()
        {
            Name = device.Name,
            State = device.State,
            Rssi = device.Rssi,
            AdvertisementRecords = device.AdvertisementRecords,
            SupportsIsConnectable = device.SupportsIsConnectable,
            IsConnectable = device.IsConnectable,
            BondState = device.BondState,
            Time = DateTimeOffset.UtcNow,
            RecordEvent = adapterDeviceEvent,
        };
        Records.Add(record);
        // State = device.State;
        if (!string.IsNullOrWhiteSpace(device.Name))
            Name = device.Name;
        Rssi = device.Rssi;
        byte a = (byte)int.Clamp(256 + Rssi*2, 0, 255);
        SignalColor = new Color(a, a,a);
        // if (device.AdvertisementRecords.Any())
        //     AdvertisementRecords = device.AdvertisementRecords;
        // IsConnectable = device.IsConnectable;
        // SupportsIsConnectable = device.SupportsIsConnectable;
        // BondState = device.BondState;
        // LastUpdate = DateTimeOffset.UtcNow;
        // LastEvent = adapterDeviceEvent;
    }
}

public enum AdapterDeviceEvent
{
    Discovered,
    Advertised,
    Connected,
    Disconnected,
    None
}