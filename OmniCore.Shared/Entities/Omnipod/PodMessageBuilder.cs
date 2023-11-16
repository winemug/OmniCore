using OmniCore.Shared.Entities.Omnipod.Parts;

namespace OmniCore.Shared.Entities.Omnipod;

public class PodMessageBuilder
{
    public uint? Address { get; private set; }
    public int? Sequence { get; private set; }
    public bool Critical { get; private set; }

    // public List<IMessagePart> Parts = new List<IMessagePart>();
    
    private byte[] _buffer = new byte[512];
    private int _bufferLen = 0;
    
    public PodMessageBuilder WithAddress(uint address)
    {
        Address = address;
        return this;
    }
    
    public PodMessageBuilder WithSequence(int sequence)
    {
        Sequence = sequence;
        return this;
    }

    public PodMessageBuilder AsCritical()
    {
        Critical = true;
        return this;
    }

    public PodMessageBuilder WithPart(IMessagePart part)
    {
        return this;
    }
    
    public PodMessageBuilder WithData(byte[] data)
    {
        return this;
    }

    public bool CanBuild()
    {
        if (!Address.HasValue)
            return false;
        return true;
    }
}