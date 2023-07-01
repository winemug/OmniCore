using OmniCore.Shared.Enums;

namespace OmniCore.Shared.Sync;

public record PodDto
{
    public Guid PodId { get; init; }
    public Guid ProfileId { get; init; }
    public Guid ClientId { get; init; }
    public uint RadioAddress { get; init; }
    public MedicationType Medication { get; init; }
    public int UnitsPerMilliliter { get; init; }

    public uint? Lot { get; init; }
    public uint? Serial { get; init; }

    public DateTimeOffset Created { get; init; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? Removed { get; init; }

}