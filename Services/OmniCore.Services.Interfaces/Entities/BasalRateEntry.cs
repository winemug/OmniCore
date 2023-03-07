namespace OmniCore.Services.Interfaces.Entities;

public struct BasalRateEntry
{
    public int HalfHourCount { get; set; }
    public int PulsesPerHour { get; set; }
}