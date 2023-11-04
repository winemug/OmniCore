namespace OmniCore.Shared.Enums;

public enum RileyLinkResponse
{
    ProtocolSync = 0x00,
    UnknownCommand = 0x22,
    RxTimeout = 0xaa,
    CommandInterrupted = 0xbb,
    CommandSuccess = 0xdd
}