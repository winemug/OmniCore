namespace OmniCore.Services.Interfaces;

public class ExchangeResult
{
    public ExchangeResult(IPodMessage message)
    {
        Message = message;
        if (message != null)
            Error = CommunicationError.None;
        else
            Error = CommunicationError.Unknown;
    }

    public ExchangeResult(CommunicationError error)
    {
        Error = error;
        Message = null;
    }

    public CommunicationError Error { get; }
    public IPodMessage Message { get; }
}