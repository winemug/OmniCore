namespace OmniCore.Client.Mobile.Implementations;

public struct PodUnits
{
    public long MilliPulses { get; }
    public int DeciPulses { get; }
    public int Pulses { get; }

    public PodUnits(Decimal internationalUnits, int unitsPerMilliliter, int pulseVolumeNanoliters = 500)
    {
        // var pulsesPerMilliter = (1000 * 1000) / pulseVolumeNanoliters;
        // var unitsPerPulses = unitsPerMilliliter / pulsesPerMilliter;
        // var totalPulses = internationalUnits / unitsPerPulses;

        // var totalPulses = internationalUnits / (unitsPerMilliliter / ((1000 * 1000) / pulseVolumeNanoliters));
        
        // var totalPulses = (internationalUnits * ((1000 * 1000) / pulseVolumeNanoliters)) / (unitsPerMilliliter);
        
        //var totalPulses = (internationalUnits * 1000 * 1000 / pulseVolumeNanoliters) / (unitsPerMilliliter);
        // var totalPulses = (internationalUnits * 1000 * 1000) / (pulseVolumeNanoliters * unitsPerMilliliter);
        // var totalPulses = internationalUnits * 1000000 / (pulseVolumeNanoliters * unitsPerMilliliter);
        MilliPulses = (long)(internationalUnits * 1000000000m / pulseVolumeNanoliters / unitsPerMilliliter);
        DeciPulses = (int)(MilliPulses / 100);
        Pulses = DeciPulses / 10;
    }
}