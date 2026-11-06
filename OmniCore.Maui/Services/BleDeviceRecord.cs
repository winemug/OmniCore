using Plugin.BLE.Abstractions;
using Plugin.BLE.Abstractions.Contracts;

namespace OmniCore.Maui.Services;

public record BleDeviceRecord
{
    public BleDeviceEvent Event { get; set; }
    public bool Connected { get; set; }
    public int Rssi { get; set; }
    public DateTimeOffset Time { get; set; }
    public DeviceState State { get; set; }
    public DeviceBondState BondState { get; set; }
}

public enum BleDeviceEvent
{
    Advertised,
    Discovered,
    Connected,
    ConnectionError,
    ConnectionLost,
    BondStateChanged,
    Disconnected
}
