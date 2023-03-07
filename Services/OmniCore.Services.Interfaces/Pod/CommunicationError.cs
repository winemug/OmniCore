namespace OmniCore.Services.Interfaces.Pod;

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