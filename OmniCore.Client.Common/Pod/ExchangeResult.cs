using OmniCore.Shared.Enums;

namespace OmniCore.Common.Pod;

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
            Result = result.Value;
        if (errorText != null)
            ErrorText = errorText;
        return this;
    }
}