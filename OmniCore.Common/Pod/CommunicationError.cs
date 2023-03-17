namespace OmniCore.Services.Interfaces.Pod;

public enum CommunicationError
{
    None = 0,
    NoResponse = 1,
    ConnectionInterrupted = 2,
    MessageSyncRequired = 3,
    ProtocolError = 4,
    UnidentifiedResponse = 5,
    Unknown = 6
}