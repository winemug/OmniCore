using System;
using System.Collections.Generic;
using System.Text;

namespace OmniCore.Radios.RileyLink
{
    public enum RileyLinkResponseType
    {
        Timeout = 0xaa,
        Interrupted = 0xbb,
        NoData = 0xcc,
        OK = 0xdd
    }
}
