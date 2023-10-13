namespace OmniCore.Client.Mobile.Implementations;

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