using OmniCore.Shared.Enums;

namespace OmniCore.Client.Model;

public class Pod
{
    public Guid PodId { get; set; }
    public Guid ProfileId { get; set; }
    public Guid ClientId { get; set; }
    public uint RadioAddress { get; set; }
    public MedicationType Medication { get; set; }
    public int UnitsPerMilliliter { get; set; }

    public uint? Lot { get; set; }
    public uint? Serial { get; set; }

    public DateTimeOffset Created { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? Removed { get; set; }
    public DateTimeOffset LastUpdated { get; set; } = DateTimeOffset.UtcNow;

    public bool IsSynced { get; set; }
    //public List<PodAction> Actions { get; set; }
}