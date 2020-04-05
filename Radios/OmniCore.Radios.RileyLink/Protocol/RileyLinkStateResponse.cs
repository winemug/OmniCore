using System;
using System.Collections.Concurrent;
using OmniCore.Radios.RileyLink.Enumerations;

namespace OmniCore.Radios.RileyLink.Protocol
{
    public class RileyLinkStateResponse : RileyLinkDefaultResponse
    {
        public bool StateOk { get; private set; }
        protected override void ParseInternal(byte[] responseData)
        {
            StateOk = (responseData.Length == 2) &&
                      (responseData[0] == 'O') &&
                      (responseData[1] == 'K');
        }
    }
}