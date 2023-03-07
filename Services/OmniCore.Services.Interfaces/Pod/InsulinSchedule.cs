namespace OmniCore.Services.Interfaces.Pod;

public struct InsulinSchedule
{
    public int BlockCount { get; set; } // 8 bits
    public bool AddAlternatingExtraPulse { get; set; }
    public int PulsesPerBlock { get; set; } // 10bits
}