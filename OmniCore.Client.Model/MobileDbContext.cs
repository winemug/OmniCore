using Microsoft.EntityFrameworkCore;

namespace OmniCore.Client.Model;

public class MobileDbContext : DbContext
{
    public DbSet<User> Users { get; set; } = null!;
    public DbSet<Profile> Profiles { get; set; } = null!;
    public DbSet<Pod> Pods { get; set; } = null!;
    public DbSet<PodAction> PodActions { get; set; } = null!;
    public DbSet<Radio> Radios { get; set; } = null!;
    
    public MobileDbContext(DbContextOptions<MobileDbContext> dbContextOptions):
        base(dbContextOptions) {}

//     protected override async void OnConfiguring(DbContextOptionsBuilder options)
//     {
// #if DEBUG
//         options.EnableDetailedErrors().EnableSensitiveDataLogging();
// #endif
//     }
}