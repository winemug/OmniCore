using OmniCore.Model.Interfaces.Services;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OmniCore.Model.Interfaces.Attributes
{
    public interface IRadioAttributes
    {
        string ProviderSpecificId { get; set; }
        Guid DeviceUuid { get; set; }
        string DeviceIdReadable { get; set; }
        string DeviceName { get; set; }
        string UserDescription { get; set; }
        string ProviderSpecificConfiguration { get; set; }
    }
}
