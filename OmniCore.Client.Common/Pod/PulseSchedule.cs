using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmniCore.Common.Pod
{
    public struct PulseSchedule
    {
        public ushort CountDecipulses { get; set; }
        public ulong IntervalMicroseconds { get; set; }
    }
}
