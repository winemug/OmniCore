using System.Collections.Concurrent;
using System.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OmniCore.Maui.Models;
using Plugin.BLE.Abstractions;
using Plugin.BLE.Abstractions.Contracts;

namespace OmniCore.Maui.Services;

public partial class BleDevice(Guid id) : ObservableObject
{
    public Guid Id { get; } = id;

    public string FriendlyId =>
        string.Join(':', Id.ToByteArray(true)[new Range(10, 16)] .Select(b => b.ToString("X2")));

    public List<BleDeviceCharacteristic> Characteristics { get; } = new();
    public ConcurrentDictionary<AdvertisementRecordType, byte[]> AdvertisementRecords { get; } = new();
    public List<BleDeviceRecord> StateRecords { get; init; } = new();
    
    [ObservableProperty] private string? _name;

    public void AddDeviceEvent(BleDeviceEvent deviceEvent, IDevice device)
    {
        if (!string.IsNullOrWhiteSpace(device.Name))
            Name = device.Name;

        if (device.AdvertisementRecords != null)
            foreach (var record in device.AdvertisementRecords)
            {
                AdvertisementRecords.AddOrUpdate(record.Type,
                    (_) => record.Data,
                    (_, _) => record.Data);
            }
        
        StateRecords.Add(new BleDeviceRecord
        {
            Event = deviceEvent,
            State = device.State,
            BondState = device.BondState,
            Rssi = device.Rssi,
            Time = DateTimeOffset.Now
        });
    }
    
    private async Task DiscoverServices(IDevice device)
    {
        try
        {
            Debug.WriteLine("Discovering services..");
            Characteristics.Clear();
            var services = await device.GetServicesAsync();
            if (services != null)
            {
                foreach (var service in services)
                {
                    var characteristics = await service.GetCharacteristicsAsync();
                    if (characteristics != null)
                        foreach (var characteristic in characteristics)
                        {
                            Characteristics.Add(new BleDeviceCharacteristic(characteristic));
                        }
                }
            }
            _discoveryFinished = true;
            Debug.WriteLine("End of service discovery.");
        }
        catch (Exception e)
        {
            Debug.WriteLine($"Error during service discovery: {e.Message}");
            _discoveryFinished = false;
        }
    }
}
