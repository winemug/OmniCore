using System;
using System.Collections.Concurrent;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using OmniCore.Radios.RileyLink.Enumerations;

namespace OmniCore.Radios.RileyLink.Protocol
{
    public class RileyLinkResponse : IRileyLinkResponse
    {
        public RileyLinkResponseType ResponseType { get; set; }

        public void Parse(byte[] responseData)
        {
            ResponseType = (RileyLinkResponseType) responseData[0];

            if (ResponseType == RileyLinkResponseType.Ok && responseData.Length > 1)
                ParseResponse(responseData[1..]);
        }

        protected virtual void ParseResponse(byte[] responseData)
        {
        }
    }
}