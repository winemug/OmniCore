using OmniCore.Radios.RileyLink.Enumerations;

namespace OmniCore.Radios.RileyLink.Protocol
{
    public interface IRileyLinkCommand
    {
        RileyLinkCommandType Type { get; }
        byte[] Parameters { get; }
    }
}