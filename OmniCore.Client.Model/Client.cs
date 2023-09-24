namespace OmniCore.Client.Model;

public class Client
{
    public Guid ClientId { get; set; }

    public string? Name { get; set; }
    public DateTimeOffset Created { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset LastUpdated { get; set; } = DateTimeOffset.UtcNow;
    public bool IsDeleted { get; set; }

    public bool IsSynced { get; set; }
}