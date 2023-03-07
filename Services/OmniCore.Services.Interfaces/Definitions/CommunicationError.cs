namespace OmniCore.Services.Interfaces;

public enum CommunicationError
{
    None,
    NoResponse,
    ConnectionInterrupted,
    MessageSyncRequired,
    ProtocolError,
    UnidentifiedResponse,
    Unknown,
}