namespace OmniCore.Common.Pod;

public class InsulinSchedule
{
    public InsulinScheduleEntry[] Entries { get; set; }
    public ulong InitialDurationMilliseconds { get; set; }
    public ushort InitialDurationPulseCount { get; set; }
    public byte HalfHourIndicator { get; set; }
}