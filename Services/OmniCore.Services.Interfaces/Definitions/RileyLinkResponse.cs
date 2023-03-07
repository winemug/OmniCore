namespace OmniCore.Services.Interfaces.Definitions;

public enum RileyLinkResponse
{
    ProtocolSync = 0x00,
    UnknownCommand = 0x22,
    RxTimeout = 0xaa,
    CommandInterrupted = 0xbb,
    CommandSuccess = 0xdd,
    NoResponse = 0xff
}