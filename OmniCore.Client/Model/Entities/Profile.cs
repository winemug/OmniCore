namespace OmniCore.Client.Model.Entities;

public class Profile
{
    public Guid ProfileId { get; set; }
    public string Name { get; set; } = null!;
    public DateTimeOffset Created { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset LastUpdated { get; set; } = DateTimeOffset.UtcNow;
    public bool IsDeleted { get; set; }
    public bool IsSynced { get; set; }
}