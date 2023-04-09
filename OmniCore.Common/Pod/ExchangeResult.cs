using OmniCore.Services.Interfaces.Entities;
using OmniCore.Services.Interfaces.Radio;

namespace OmniCore.Services.Interfaces.Pod;

public class BleExchangeResult
{ 
    public BleCommunicationResult CommunicationResult { get; set; }
    public DateTimeOffset? BleWriteCompleted { get; set; }
    public DateTimeOffset? BleReadIndicated { get; set; }
    public RileyLinkResponse? ResponseCode { get; set; }
    public Bytes? ResponseData { get; set; }
    public Exception? Exception { get; set; }
}

public class ExchangeResult
{
    public DateTimeOffset? RequestSentEarliest { get; set; }
    public DateTimeOffset? RequestSentLatest { get; set; }
    public IPodMessage? SentMessage { get; set; }
    public IPodMessage? ReceivedMessage { get; set; }
    public AcceptanceType Result { get; set; }
    public string? ErrorText { get; set; }

    public ExchangeResult WithResult(AcceptanceType? result = default, string? errorText = default)
    {
        if (result.HasValue)
            this.Result = result.Value;
        if (errorText != null)
            this.ErrorText = errorText;
        return this;
    }
}

public enum AcceptanceType
{
    Accepted,
    Inconclusive,
    Ignored,
    RejectedResyncRequired,
    RejectedProtocolError,
    RejectedNonceReseed,
    RejectedErrorOccured,
    RejectedFaultOccured,
}

public class BlePacketExchangeResult
{
    public IPodPacket Sent { get; set; }
    public IPodPacket? Received { get; set; }
    public bool BleConnectionSuccessful { get; set; }
}

public enum BleCommunicationResult
{
    OK,
    WriteFailed,
    IndicateTimedOut,
    ReadFailed,
}

