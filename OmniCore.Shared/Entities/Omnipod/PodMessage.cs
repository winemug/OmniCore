namespace OmniCore.Shared.Entities.Omnipod;

public class PodMessage
{
    public uint Address { get; init; }

    public int Sequence { get; init; }
    public bool Critical { get; init; }
    
    public IMessagePart[] Parts;
}