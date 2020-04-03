using System;
using OmniCore.Radios.RileyLink.Enumerations;

namespace OmniCore.Radios.RileyLink.Protocol
{
    public interface IRileyLinkCommand
    {
        RileyLinkCommandType CommandType { get; set; }
        byte[] Parameters { get; set; }
        bool HasResponse { get; }
        void ParseResponse(byte[] data);
        void SetTransmissionResult(Exception e);
    }
}