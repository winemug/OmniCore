using System;
using System.Collections.Concurrent;
using System.Reactive.Subjects;
using OmniCore.Radios.RileyLink.Enumerations;

namespace OmniCore.Radios.RileyLink.Protocol
{
    public interface IRileyLinkResponse
    {
        RileyLinkResult Result { get; }
        void Parse(byte[] responseData);
        bool SkipParse { get; set; }
        IObservable<IRileyLinkResponse> Observable { get; }
    }
}