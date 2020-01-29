using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Innofactor.EfCoreJsonValueConverter;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using OmniCore.Model.Entities;
using OmniCore.Model.Enumerations;
using OmniCore.Model.Interfaces.Data;
using OmniCore.Model.Interfaces.Platform.Common;

namespace OmniCore.Repository
{
    public class RepositoryContext : DbContext, IRepositoryContext
    {
        public DbSet<MedicationEntity> Medications { get; set; }
        public DbSet<UserEntity> Users { get; set; }
        public DbSet<RadioEntity> Radios { get; set; }
        public DbSet<RadioEventEntity> RadioEvents { get; set; }
        public DbSet<PodEntity> Pods { get; set; }
        public DbSet<PodRequestEntity> PodRequests { get; set; }
        
        public DbSet<MedicationDeliveryEntity> MedicationDeliveries { get; set; }

        public DbSet<PodResponseEntity> PodResponses { get; set; }

        public Task Save(CancellationToken cancellationToken)
        {
            return SaveChangesAsync(cancellationToken);
        }

        public readonly string ConnectionString;

        // for migrations tool
        public RepositoryContext()
        {
            ConnectionString = $"Data Source=:memory:";
        }
        
        public RepositoryContext(ICoreApplicationFunctions applicationFunctions)
        {
            var path = Path.Combine(applicationFunctions.DataPath, "oc.db3");
            ConnectionString = $"Data Source={path}";
        }

        public async Task SeedData()
        {
            if (!Medications.Any())
            {
                Medications.Add(new MedicationEntity
                {
                    Hormone = HormoneType.Unknown,
                    Name = "Unspecified",
                    UnitName = "microliters",
                    UnitNameShort = "µL",
                    UnitsPerMilliliter = 1000
                });
            }

            if (!Users.Any())
            {
                Users.Add(new UserEntity
                {
                    Name = "First User",
                });
            }
            await SaveChangesAsync();
            
            #if DEBUG

            if (!Radios.Any())
            {
                Radios.Add(new RadioEntity
                {
                    DeviceUuid = Guid.Parse("00000000-0000-0000-0000-886b0ff93ba7"),
                    UserDescription = "greenie"
                });
                await SaveChangesAsync();
            }

            if (!Pods.Any())
            {
                Pods.Add(new PodEntity
                {
                    Lot = 1,
                    Serial = 1,
                    RadioAddress = 0x11121212,
                    Radio = Radios.First(),
                    User = Users.First()
                });
                await SaveChangesAsync();
            }
            
            #endif
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite(ConnectionString);
            #if DEBUG
            optionsBuilder.EnableDetailedErrors(true);
            #endif
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.AddJsonFields();
        }
    }
}