using OmniCore.Model.Constants;
using System;
using System.Collections.Generic;
using System.Text;

namespace OmniCore.Model
{
    public class BasalEntry
    {
        public decimal Rate { get; }
        public TimeSpan StartOffset { get; }
        public TimeSpan EndOffset { get; }
        public BasalEntry(decimal hourlyRate, TimeSpan startOffsetFromMidnight, TimeSpan endOffsetFromMidnight)
        {
            if (hourlyRate < Limits.MinimumRate)
                throw new ArgumentException($"Basal rate cannot be less than {Limits.MinimumRate}");

            if (hourlyRate > Limits.MaximumRate)
                throw new ArgumentException($"Basal rate cannot be more than {Limits.MaximumRate}");

            if (hourlyRate % Limits.RateIncrements != 0)
                throw new ArgumentException($"Basal rate must be increments of {Limits.RateIncrements} units");

            if (startOffsetFromMidnight >= endOffsetFromMidnight)
                throw new ArgumentException("Start offset must come before end offset");

            if (!VerifyTimeBoundary(startOffsetFromMidnight))
                throw new ArgumentException($"Start offset must be at {Limits.TimeIncrements} boundary.");

            if (!VerifyTimeBoundary(endOffsetFromMidnight))
                throw new ArgumentException($"End offset must be at {Limits.TimeIncrements} boundary.");

            this.Rate = hourlyRate;
            this.StartOffset = startOffsetFromMidnight;
            this.EndOffset = endOffsetFromMidnight;
        }

        public bool VerifyTimeBoundary(TimeSpan ts)
        {
            return (ts.TotalSeconds % Limits.TimeIncrements.TotalSeconds < 1);
        }
    }
}
