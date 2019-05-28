using System;
using System.Collections.Generic;
using System.Text;

namespace OmniCore.Model.Interfaces
{
    public interface IPodRuntimeVariables
    {
        int PacketSequence { get; set; }
        uint? LastNonce { get; set; }
        int NoncePtr { get; set; }
        int NonceRuns { get; set; }
        uint NonceSeed { get; set; }
        uint? NonceSync { get; set; }
    }
}
