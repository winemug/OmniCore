using OmniCore.Model.Constants;
using System;
using System.Collections.Generic;
using System.Text;

namespace OmniCore.Model
{
    [Serializable]

    public class BasalSchedule
    {
        public decimal?[] Table { get; set; }

        public BasalSchedule()
        { }

        internal BasalSchedule(decimal fixedRate) :
            this(new List<BasalEntry>
                { new BasalEntry(fixedRate, TimeSpan.Zero, TimeSpan.FromHours(24)) })
        {
        }

        public BasalSchedule(IEnumerable<BasalEntry> basalEntries)
        {
            var tableSize = (int) Math.Round(TimeSpan.FromDays(1).TotalSeconds / Limits.TimeIncrements.TotalSeconds);
            this.Table = new decimal?[tableSize];

            foreach (var entry in basalEntries)
            {
                var startIndex = (int) Math.Round(entry.StartOffset.TotalSeconds / Limits.TimeIncrements.TotalSeconds);
                var endIndex = (int) Math.Round(entry.EndOffset.TotalSeconds / Limits.TimeIncrements.TotalSeconds);

                for (int i=startIndex; i<endIndex; i++)
                {
                    if (this.Table[i].HasValue)
                        throw new ArgumentException($"Cannot construct a schedule with overlapping basal entries. Related entry: {entry}");
                    this.Table[i] = entry.Rate;
                }
            }

            foreach(var rate in this.Table)
            {
                if (!rate.HasValue)
                    throw new ArgumentException($"Gap found in basal entries. The entire day should be covered.");
            }
        }       
    }
}
