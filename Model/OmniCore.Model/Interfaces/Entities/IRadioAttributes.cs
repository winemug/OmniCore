using System;

namespace OmniCore.Model.Interfaces.Entities
{
    public interface IRadioAttributes
    {
        string ProviderSpecificId { get; set; }
        Guid DeviceUuid { get; set; }
        string DeviceIdReadable { get; set; }
        string DeviceName { get; set; }
        string UserDescription { get; set; }
    }
}
