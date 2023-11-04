using OmniCore.Shared.Structs;

namespace OmniCore.Client.Implementation;

public class PodBasalSchedule
{
    public PodBasalSchedule WithStartTime(TimeOnly basalStartTime)
    {
        return this;
    }

    public PodBasalSchedule WithRateAndTimeInterval(PodUnitRate rate, TimeOnly intervalStart, TimeOnly intervalStop)
    {
        return this;
    }
}