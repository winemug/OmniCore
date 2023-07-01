using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using OmniCore.Shared.Enums;

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
        DbPath = Path.Join(path, "ocefcore.sqlite");
    }

    protected override void OnConfiguring(DbContextOptionsBuilder options)
    {
        options.UseSqlite($"Data Source={DbPath}"); //.EnableDetailedErrors(true).EnableSensitiveDataLogging(true);
    }
}
    
public class Account
{
    public Guid AccountId { get; set; }
    public string? Name { get; set; }
    public string? Country { get; set; }
    public string? Phone { get; set; }
    public DateTimeOffset Created { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset LastUpdated { get; set; } = DateTimeOffset.UtcNow;
    public bool IsSynced { get; set; }
}

public class Client
{
    public Guid ClientId { get; set; }
    public Guid AccountId { get; set; }
    public string Name { get; set; }
    public DateTimeOffset Created { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset LastUpdated { get; set; } = DateTimeOffset.UtcNow;
    public bool IsDeleted { get; set; }
    public bool IsSynced { get; set; }
    public Account Account { get; set; }
}

public class Profile
{
    public Guid ProfileId { get; set; }
    public Guid AccountId { get; set; }
    public string Name { get; set; } = null!;
    public DateTimeOffset Created { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset LastUpdated { get; set; } = DateTimeOffset.UtcNow;
    public bool IsDeleted { get; set; }
    public bool IsSynced { get; set; }
    public Account Account { get; set; }
}

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
    public List<PodAction> Actions { get; set; }
}


[PrimaryKey(nameof(PodId), nameof(Index))]
public class PodAction
{
    public Guid PodId { get; set; }
    public int Index { get; set; }
    public Guid ClientId { get; set; }
    public DateTimeOffset? RequestSentEarliest { get; set; }
    public DateTimeOffset? RequestSentLatest { get; set; }
    public byte[]? SentData { get; set; }
    public byte[]? ReceivedData { get; set; }
    public AcceptanceType Result { get; set; }
    public bool IsSynced { get; set; }
}
