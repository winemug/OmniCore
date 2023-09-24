namespace OmniCore.Common.Pod;

public struct InsulinScheduleEntry
{
    public int BlockCount { get; set; } // 8 bits
    public bool AddAlternatingExtraPulse { get; set; }
    public int PulsesPerBlock { get; set; } // 10bits
}