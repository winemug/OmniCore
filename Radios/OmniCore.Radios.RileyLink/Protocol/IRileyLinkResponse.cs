using System;
using System.Reactive.Subjects;
using OmniCore.Radios.RileyLink.Enumerations;

namespace OmniCore.Radios.RileyLink.Protocol
{
    public interface IRileyLinkResponse
    {
        bool SkipParse { get; set; }
        IObservable<IRileyLinkResponse> Observable { get; }
        void Parse(byte[] responseData);
    }
}