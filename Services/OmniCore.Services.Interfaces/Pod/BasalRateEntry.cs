namespace OmniCore.Services.Interfaces.Pod;

public struct BasalRateEntry
{
    public int HalfHourCount { get; set; }
    public int PulsesPerHour { get; set; }
}