namespace OmniCore.Shared.Entities;

public class PodRadioMeasurementsModel
{
    public int RadioLowGain { get; init; }
    public int Rssi { get; init; }
    private DateTimeOffset Received { get; set; }
}