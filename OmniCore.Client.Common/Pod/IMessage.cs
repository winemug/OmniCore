using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OmniCore.Common.Entities;

namespace OmniCore.Common.Pod;

public interface IMessage
{
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
    abstract static Predicate<IMessageParts> CanParse { get; }
}
