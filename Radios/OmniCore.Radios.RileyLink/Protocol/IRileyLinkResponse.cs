using System;
using OmniCore.Radios.RileyLink.Enumerations;

namespace OmniCore.Radios.RileyLink.Protocol
{
    public interface IRileyLinkResponse
    {
        RileyLinkResult Result { get; }
        bool SkipParse { get; set; }
        IObservable<IRileyLinkResponse> Observable { get; }
        void Parse(byte[] responseData);
    }
}