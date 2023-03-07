using OmniCore.Services.Interfaces;

namespace OmniCore.Services;

public class ResponseStatusPart : MessagePart
{
    public ResponseStatusPart(Bytes data)
    {
        Data = data;
        var b0 = data[0];
        ExtendedBolusActive = (b0 & 0b10000000) > 0;
        ImmediateBolusActive = (b0 & 0b01000000) > 0;
        TempBasalActive = (b0 & 0b00100000) > 0;
        BasalActive = (b0 & 0b00010000) > 0;
        Progress = (PodProgress)(b0 & 0x0F);

        var d0 = data.DWord(1);
        PulsesDelivered = (int)((d0 >> 15) & 0b000001111111111111);
        LastProgrammingCommandSequence = (int)((d0 >> 11) & 0b00001111);
        PulsesPending = (int)(d0 & 0b0000011111111111);

        var d1 = data.DWord(5);
        Faulted = (d1 & 0x80000000) != 0;
        UnackedAlertsMask = (int)((d1 >> 23) & 0x0F);
        ActiveMinutes = (int)((d1 >> 10) & 0b0001111111111111);
        var pr = (int)(d1 & 0b0000001111111111);
        if (pr == 0x3FF)
            PulsesRemaining = null;
        else
            PulsesRemaining = pr;
    }

    public override bool RequiresNonce => false;
    public override PodMessageType Type => PodMessageType.ResponseStatus;
    public bool ExtendedBolusActive { get; }
    public bool ImmediateBolusActive { get; }
    public bool TempBasalActive { get; }
    public bool BasalActive { get; }
    public PodProgress Progress { get; }
    public int PulsesDelivered { get; }
    public int PulsesPending { get; }
    public int? PulsesRemaining { get; }
    public int ActiveMinutes { get; }
    public bool Faulted { get; }
    public int UnackedAlertsMask { get; }
    public int LastProgrammingCommandSequence { get; }
}