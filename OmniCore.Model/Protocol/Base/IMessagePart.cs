using System;
using System.Collections.Generic;
using System.Text;

namespace OmniCore.Model.Protocol.Base
{
    public interface IMessagePart
    {
        bool HasLengthField { get; }
        byte PartType { get; }
        byte[] Content { get; }
        byte[] Data { get; }
        bool NonceRequired { get; }
        void SetNonce(uint nonce);
    }
}
