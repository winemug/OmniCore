using OmniCore.Common.Pod;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmniCore.Framework.Omnipod.Parts
{
    public class MessageParameters : IMessageParameters
    {
        public uint Address { get; set; }
        public int Sequence { get; set; }
        public bool Critical { get; set; }
        public INonceProvider? NonceProvider { get; set; }
    }
}
