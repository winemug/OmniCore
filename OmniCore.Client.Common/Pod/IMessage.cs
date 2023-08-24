using OmniCore.Common.Entities;

namespace OmniCore.Common.Pod;

public interface IMessage
{
    static abstract Predicate<IMessageParts> CanParse { get; }
    //uint? Address { get; set; }
    //int? Sequence { get; set; }
    //bool Critical { get; set; }
    //INonceProvider? NonceProvider { get; set; }

    //IMessage WithAddress(uint address);
    //IMessage WithSequence(int sequence);
    //IMessage AsCritical();
    //IMessage WithNonceProvider(INonceProvider nonceProvider);

    //IMessageParts GetParts();
    IMessage Parse(IMessageParts parts);
    Bytes ToBody();
}