namespace OmniCore.Radios.RileyLink.Enumerations
{
    public enum RileyLinkResponseType
    {
        Timeout = 0xaa,
        Interrupted = 0xbb,
        NoData = 0xcc,
        ParameterError = 0x11,
        UnknownCommand = 0x12,
        Ok = 0xdd
    }
}