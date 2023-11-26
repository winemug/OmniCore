using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmniCore.Client.Abstractions.Services;
public interface IBleDevice
{
    Guid Address { get; set; }
    string? Name { get; set; }
    int LastRssi { get; set; }
    DateTimeOffset LastSeen { get; set; }
    string AddressText { get; }
}
