using OmniCore.Shared.Enums;

namespace OmniCore.Shared.Entities.Omnipod;

public class AlertConfiguration
{
    public int AlertIndex { get; set; } // 3 bits
    public bool SetActive { get; set; }
    public bool ReservoirBased { get; set; }
    public bool SetAutoOff { get; set; }
    public int AlertDurationMinutes { get; set; } // 9 bits

    public int AlertAfter { get; set; }
    public BeepPattern BeepPattern { get; set; }
    public BeepType BeepType { get; set; }
}