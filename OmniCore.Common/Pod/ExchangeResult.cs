namespace OmniCore.Services.Interfaces.Pod;

public class ExchangeResult
{
    public DateTimeOffset? SendStart { get; set; }
    public DateTimeOffset? ReceiveStart { get; set; }
    public IPodMessage? SentMessage { get; set; }
    public IPodMessage? ReceivedMessage { get; set; }
    public CommunicationStatus CommunicationStatus { get; set; }
    public string ErrorText { get; set; }
}