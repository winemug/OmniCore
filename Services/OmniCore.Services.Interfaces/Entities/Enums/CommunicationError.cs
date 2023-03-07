namespace OmniCore.Services.Interfaces.Definitions;

public enum CommunicationError
{
    None,
    NoResponse,
    ConnectionInterrupted,
    MessageSyncRequired,
    ProtocolError,
    UnidentifiedResponse,
    Unknown
}