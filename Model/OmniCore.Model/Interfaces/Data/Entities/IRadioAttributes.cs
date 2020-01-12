using System;

namespace OmniCore.Model.Interfaces.Platform.Common.Data.Entities
{
    public interface IRadioAttributes
    {
        Guid DeviceUuid { get; set; }
        Guid[] ServiceUuids { get; set; }
        string DeviceName { get; set; }
        string UserDescription { get; set; }
    }
}
