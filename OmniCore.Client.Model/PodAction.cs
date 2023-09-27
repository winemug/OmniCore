using Microsoft.EntityFrameworkCore;
using OmniCore.Shared.Enums;

namespace OmniCore.Client.Model;

[PrimaryKey(nameof(PodId), nameof(Index))]
public class PodAction
{
    public Guid PodId { get; set; }
    public int Index { get; set; }
    public Guid UserId { get; set; }
    public DateTimeOffset? RequestSentEarliest { get; set; }
    public DateTimeOffset? RequestSentLatest { get; set; }
    public byte[]? SentData { get; set; }
    public byte[]? ReceivedData { get; set; }
    public AcceptanceType Result { get; set; }
    public bool IsSynced { get; set; }
}