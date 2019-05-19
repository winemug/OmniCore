using System;

namespace OmniCore.Model.Constants
{
    class Limits
    {
        internal static readonly decimal MinimumRate = 0.05m;
        internal static readonly decimal MaximumRate = 30m;
        internal static readonly decimal RateIncrements = 0.05m;
        internal static readonly TimeSpan TimeIncrements = TimeSpan.FromMinutes(30);
    }
}
