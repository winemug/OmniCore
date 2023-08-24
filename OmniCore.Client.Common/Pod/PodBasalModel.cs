namespace OmniCore.Common.Pod;

public class PodBasalModel
{
    public DateTimeOffset PodTimeReference { get; set; }
    public TimeOnly PodTimeReferenceValue { get; set; }

    public TimeOnly PodTimeNow
    {
        get
        {
            var timeDifference = DateTimeOffset.UtcNow - PodTimeReference;
            return PodTimeReferenceValue.Add(timeDifference);
        }
    }

    public int[] BasalSchedule { get; set; }

    public TimeOnly PodTimeThen(DateTimeOffset when)
    {
        var timeDifference = DateTimeOffset.UtcNow - when;
        return PodTimeReferenceValue.Add(timeDifference);
    }
}