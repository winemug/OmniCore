using OmniCore.Shared.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmniCore.Shared.Entities.Omnipod.Parts;

public interface IMessagePart
{
    int ToBytes(Span<byte> span);
}
