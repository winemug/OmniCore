using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmniCore.Shared.Entities.Omnipod.Parts;
public record PulseInterval
{
    public ushort Pulse10Count { get; set; }
    public uint IntervalMicroseconds { get; set; }
}
