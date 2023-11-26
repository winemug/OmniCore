using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace OmniCore.Client.Model;

public class MobileDbContextFactory : IDesignTimeDbContextFactory<MobileDbContext>
{
    public MobileDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<MobileDbContext>();
        optionsBuilder.UseSqlite($"Data Source={args[1]}");

        return new MobileDbContext(optionsBuilder.Options);

    }
}