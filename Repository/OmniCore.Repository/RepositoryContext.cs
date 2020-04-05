using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Innofactor.EfCoreJsonValueConverter;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Logging;
using OmniCore.Model.Entities;
using OmniCore.Model.Enumerations;
using OmniCore.Model.Interfaces.Services;
using OmniCore.Model.Interfaces.Services.Internal;

namespace OmniCore.Repository
{
    public class RepositoryContext : DbContext, IRepositoryContextReadWrite
    {
        public DbSet<MedicationEntity> Medications { get; set; }
        public DbSet<UserEntity> Users { get; set; }
        public DbSet<RadioEntity> Radios { get; set; }
        public DbSet<RadioEventEntity> RadioEvents { get; set; }
        public DbSet<PodEntity> Pods { get; set; }
        public DbSet<PodRequestEntity> PodRequests { get; set; }
        
        public DbSet<MedicationDeliveryEntity> MedicationDeliveries { get; set; }

        public DbSet<PodResponseEntity> PodResponses { get; set; }

        private readonly string ConnectionString;
        private IDisposable ReaderWriterLock;

        public async Task InitializeDatabase(CancellationToken cancellationToken, bool createNew = false)
        {
            if (createNew)
            {
                Database.EnsureDeleted();
            }
            await Database.MigrateAsync(cancellationToken);
            await SeedData();            
        }


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

        private async Task SeedData()
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
                    Name = "Default User",
                });
            }
            await SaveChangesAsync();
        }

        //public static readonly ILoggerFactory DebugLoggerFactory
        //    = LoggerFactory.Create(builder => { builder.AddDebug() ; });

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite(ConnectionString);
            #if DEBUG
            optionsBuilder.EnableDetailedErrors(true);
            //optionsBuilder.UseLoggerFactory(DebugLoggerFactory);
            #endif
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.AddJsonFields();
        }

        public override void Dispose()
        {
            base.Dispose();
            ReaderWriterLock?.Dispose();
            ReaderWriterLock = null;
        }

        public IRepositoryContextReadWrite WithExisting(Entity entity)
        {
            Attach(entity).State = EntityState.Unchanged;
            return this;
        }
        
        public Task Save(CancellationToken cancellationToken)
        {
            return SaveChangesAsync(cancellationToken);
        }

        public void SetLock(IDisposable readerWriterLock, bool tracking)
        {
            ReaderWriterLock = readerWriterLock;
            ChangeTracker.QueryTrackingBehavior = tracking ?
                QueryTrackingBehavior.TrackAll : QueryTrackingBehavior.NoTracking;
        }
    }
}