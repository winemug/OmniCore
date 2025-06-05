using Plugin.BLE.Abstractions;
using Plugin.BLE.Abstractions.Contracts;

namespace OmniCore.Maui.Services;

public record BleDeviceRecord
{
    public string Name { get; set; }
    public int Rssi { get; set; }
    public DeviceState State { get; set; }
    public IReadOnlyList<AdvertisementRecord> AdvertisementRecords { get; set; }
    public bool IsConnectable { get; set; }
    public bool SupportsIsConnectable { get; set; }
    public DeviceBondState BondState { get; set; }
    public DateTimeOffset Time { get; set; }
    public AdapterDeviceEvent RecordEvent { get; set; }
}