using OmniCore.Radios.RileyLink.Enumerations;

namespace OmniCore.Radios.RileyLink.Protocol
{
    public class RileyLinkCommand : IRileyLinkCommand
    {
        public RileyLinkCommandType Type { get; set; }
        public byte[] Parameters { get; set; }
    }
}