using OmniCore.Services.Interfaces.Entities;
using OmniCore.Services.Interfaces.Radio;

namespace OmniCore.Services.Interfaces.Pod;

public class ExchangeResult
{
    public DateTimeOffset? SendStart { get; set; }
    public DateTimeOffset? ReceiveStart { get; set; }
    public IPodMessage? SentMessage { get; set; }
    public IPodMessage? ReceivedMessage { get; set; }

    public RequestSendResult SendResult { get; set; }
    public RequestAcknowledgementResult AcknowledgementResult { get; set; }
    public ResponseReceiveResult ReceiveResult { get; set; }
    public CommunicationStatus Status { get; set; }
    
    public string ErrorText { get; set; }
}

public class BleExchangeResult
{
    public BleCommunicationResult CommunicationResult { get; init; }
    public RileyLinkResponse? ResponseCode { get; init; }
    public Bytes? ResponseData { get; init; }
}

public enum BleCommunicationResult
{
    OK,
    SendFailed,
    ReceiveTimedOut,
    ReceiveFailed,
}

public enum RequestSendResult
{
    FullySent,
    PartiallySent,
    NothingSent,
}

public enum RequestAcknowledgementResult
{
    Acknowledged,
    Inconclusive,
    Rejected,
    None,
}

public enum ResponseReceiveResult
{
    FullyReceived,
    PartiallyReceived,
    NothingReceived,
}