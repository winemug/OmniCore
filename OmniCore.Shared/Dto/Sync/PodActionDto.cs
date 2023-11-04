using OmniCore.Shared.Enums;

namespace OmniCore.Shared.Dto.Sync;

public record PodActionDto
{
    public Guid PodId { get; init; }
    public int Index { get; init; }
    public Guid ClientId { get; init; }
    public DateTimeOffset? RequestSentEarliest { get; init; }
    public DateTimeOffset? RequestSentLatest { get; init; }
    public byte[]? SentData { get; init; }
    public byte[]? ReceivedData { get; init; }
    public AcceptanceType Result { get; init; }
}