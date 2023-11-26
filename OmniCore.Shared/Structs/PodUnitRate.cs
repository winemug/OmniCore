﻿namespace OmniCore.Shared.Structs;

public struct PodUnitRate
{
    public long MilliPulsesPerHour { get; }

    public PodUnitRate(PodUnits unitsPerHour)
    {
        MilliPulsesPerHour = unitsPerHour.MilliPulses;
    }

    public PodUnitRate(PodUnits totalUnits, TimeSpan timeSpan)
    {
        MilliPulsesPerHour = (long)(totalUnits.MilliPulses / timeSpan.TotalHours);
    }

    public PodUnitRate(PodUnits totalUnits, int halfHours)
    {
        MilliPulsesPerHour = totalUnits.MilliPulses * 2 / halfHours;
    }
}