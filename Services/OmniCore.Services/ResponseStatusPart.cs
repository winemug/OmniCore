using OmniCore.Services.Interfaces;

namespace OmniCore.Services;

public class ResponseStatusPart : RadioMessagePart
{
    public override RadioMessageType Type => RadioMessageType.ResponseStatus;
    public bool ExtendedBolusActive { get; }
    public bool ImmediateBolusActive { get; }
    public bool TempBasalActive { get; }
    public bool BasalActive { get; }
    public PodProgress Progress { get; }
    public ResponseStatusPart(Bytes data)
    {
        Data = data;

        var b = data[0];
        ExtendedBolusActive = (data[0] & 0b10000000) > 0;
        ImmediateBolusActive = (data[0] & 0b01000000) > 0;
        TempBasalActive = (data[0] & 0b00100000) > 0;
        BasalActive = (data[0] & 0b00010000) > 0;
        Progress = (PodProgress)(data[0] & 0x0F);
    }
}