using OmniCore.Model.Enums;
using OmniCore.Model.Protocol.Base;
using System;
using System.Collections.Generic;
using System.Text;

namespace OmniCore.Model.Protocol.Commands
{
    public class StatusRequest : Message
    {
        public StatusRequest(StatusRequestType statusRequestType = StatusRequestType.Standard)
        {
            base.Parts = new MessagePart[] { new MessagePart(true, false, 0x0e, new byte[] { (byte)statusRequestType }) };
        }
    }
}
