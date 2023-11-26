namespace OmniCore.Client.Model.Entities;

public class User
{
    public Guid UserId { get; set; }
    public string? Name { get; set; }
    public DateTimeOffset Created { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset LastUpdated { get; set; } = DateTimeOffset.UtcNow;
    public bool IsDeleted { get; set; }
    public bool IsSynced { get; set; }
}