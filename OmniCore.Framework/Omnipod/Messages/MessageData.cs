using OmniCore.Common.Pod;
using OmniCore.Services.Interfaces.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmniCore.Framework.Omnipod.Messages
{
    public class MessageData : IMessageData
    {
        public uint Address { get; }

        public int Sequence { get; }

        public bool Critical { get; }

        public INonceProvider? NonceProvider { get; }

        public Bytes? ByteData { get; }
    }
}
