
using OmniCore.Maui.Services;
using Plugin.BLE.Abstractions.Contracts;

public record BleDeviceCharacteristic
{
    public Guid ServiceId { get; init; }
    public Guid CharacteristicId { get; init; }
    public BleDeviceCharacteristic(ICharacteristic characteristic)
    {
        CharacteristicId = characteristic.Id;
        ServiceId = characteristic.Service.Id;
    }
}