namespace OmniCore.Radio.RileyLink
{
    public enum RileyLinkResponseType
    {
        Timeout = 0xaa,
        Interrupted = 0xbb,
        NoData = 0xcc,
        OK = 0xdd
    }
}
