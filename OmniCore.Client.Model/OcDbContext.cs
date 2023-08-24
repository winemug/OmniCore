using Microsoft.EntityFrameworkCore;

namespace OmniCore.Client.Model;

public class OcDbContext : DbContext
{
    public OcDbContext()
    {
        var folder = Environment.SpecialFolder.LocalApplicationData;
        var path = Environment.GetFolderPath(folder);
        DbPath = Path.Join(path, "ocefcore.sqlite");
    }

    //public DbSet<Account> Accounts { get; set; } = null!;
    public DbSet<Client> Clients { get; set; } = null!;
    public DbSet<Profile> Profiles { get; set; } = null!;
    public DbSet<Pod> Pods { get; set; } = null!;
    public DbSet<PodAction> PodActions { get; set; } = null!;
    public DbSet<Radio> Radios { get; set; } = null!;
    private string DbPath { get; }

    protected override async void OnConfiguring(DbContextOptionsBuilder options)
    {
        options.UseSqlite($"Data Source={DbPath}");
#if DEBUG
        options.EnableDetailedErrors().EnableSensitiveDataLogging();
#endif
        await Database.EnsureCreatedAsync();
    }
}

// public class Account
// {
//     public Guid AccountId { get; set; }
//     public string? Name { get; set; }
//     public string? Country { get; set; }
//     public string? Phone { get; set; }
//     public DateTimeOffset Created { get; set; } = DateTimeOffset.UtcNow;
//     public DateTimeOffset LastUpdated { get; set; } = DateTimeOffset.UtcNow;
//     public bool IsSynced { get; set; }
// }