using OmniCore.Model.Enums;
using OmniCore.Model.Protocol.Base;
using System;
using System.Collections.Generic;
using System.Text;

namespace OmniCore.Model.Protocol.Responses
{
    public class InformationResponse : Message
    {
        // https://github.com/openaps/openomni/wiki/Command-02-Pod-Information-Response
        public InformationResponse(IEnumerable<IMessagePart> parts)
        {
        }
    }
}
