using Microsoft.EntityFrameworkCore;
using OmniCore.Services.Interfaces.Pod;

namespace OmniCore.Common.Data;

public class OcdbContext : DbContext
{
    public DbSet<Account> Accounts { get; set; } = null!;
    public DbSet<Client> Clients { get; set; } = null!;
    public DbSet<Profile> Profiles { get; set; } = null!;
    public DbSet<Pod> Pods { get; set; } = null!;
    public DbSet<PodAction> PodActions { get; set; } = null!;
    public string DbPath { get; }
    public OcdbContext()
    {
        var folder = Environment.SpecialFolder.LocalApplicationData;
        var path = Environment.GetFolderPath(folder);
        DbPath = Path.Join(path, "ocefcore.sqlite3");
    }

    protected override void OnConfiguring(DbContextOptionsBuilder options)
    {
        options.UseSqlite($"Data Source={DbPath}");
    }
}

[Index(nameof(Email), IsUnique = true)]
public class Account
{
    public Guid AccountId { get; set; }
    public string Email { get; set; } = null!;
    public string? Password { get; set; }
    public string? Name { get; set; }
    public string? Country { get; set; }
    public string? Phone { get; set; }
    public string? VerificationKey { get; set; }
    public bool IsVerified { get; set; }
    public bool IsActive { get; set; }
    public DateTimeOffset Created { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset LastUpdated { get; set; } = DateTimeOffset.UtcNow;
    public bool IsSynced { get; set; }
    public List<Client> Clients { get; set; }
}

public class Client
{
    public Guid ClientId { get; set; }
    public Guid AccountId { get; set; }
    public string Name { get; set; }
    public string? ApiToken { get; set; }
    public DateTimeOffset Created { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset LastUpdated { get; set; } = DateTimeOffset.UtcNow;
    public bool IsDeleted { get; set; }
    public bool IsSynced { get; set; }
}

[Index(nameof(AccountId), nameof(Name), IsUnique = true)]
public class Profile
{
    public Guid ProfileId { get; set; }
    public Guid AccountId { get; set; }
    public string Name { get; set; } = null!;
    public DateTimeOffset Created { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset LastUpdated { get; set; } = DateTimeOffset.UtcNow;
    public bool IsDeleted { get; set; }
    public bool IsSynced { get; set; }
}

public class Pod
{
    public Guid PodId { get; set; }
    public Guid ProfileId { get; set; }
    public Guid ClientId { get; set; }
    public uint RadioAddress { get; set; }
    public MedicationType Medication { get; set; }
    public DateTimeOffset Created { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? Removed { get; set; }
    public DateTimeOffset LastUpdated { get; set; } = DateTimeOffset.UtcNow;
    public bool IsSynced { get; set; }
}

[PrimaryKey(nameof(PodId), nameof(Index))]
public class PodAction
{
    public Guid PodId { get; set; }
    public int Index { get; set; }
    public DateTimeOffset SendTime { get; set; }
    public byte[] Sent { get; set; } = null!;
    public DateTimeOffset? ReceivedTime { get; set; }
    public byte[]? Received { get; set; } 
    public bool IsSynced { get; set; }
}
