using OmniCore.Repository.Entities;
using OmniCore.Repository.Enums;
using SQLite;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OmniCore.Repository
{
    public class RepositoryProvider
    {
        private RepositoryProvider() { }

        public static RepositoryProvider Instance { get; } = new RepositoryProvider();

        private SQLiteAsyncConnection Connection => new SQLiteAsyncConnection(DatabasePath, SQLiteOpenFlags.Create | SQLiteOpenFlags.ReadWrite);
        public string DatabasePath;

        public void Init()
        {
            Task.Run(async () => await Instance.Initialize()).Wait();
        }

        public MedicationRepository MedicationRepository => new MedicationRepository(Connection);
        public UserRepository UserRepository => new UserRepository(Connection);
        public UserProfileRepository UserProfileRepository => new UserProfileRepository(Connection);
        public RadioRepository RadioRepository => new RadioRepository(Connection);
        public PodRepository PodRepository => new PodRepository(Connection);
        public PodRequestRepository PodRequestRepository => new PodRequestRepository(Connection);
        public RadioConnectionRepository RadioConnectionRepository => new RadioConnectionRepository(Connection);

    private async Task Initialize()
        {
            DatabasePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "omnicore.db3");
#if DEBUG
            //if (File.Exists(DatabasePath))
            //    File.Delete(DatabasePath);
#endif
            //TODO: lame repo inits, to be ioc'ed later
            await MedicationRepository.Initialize();
            await UserRepository.Initialize();
            await UserProfileRepository.Initialize();
            await RadioRepository.Initialize();
            await PodRepository.Initialize();
            await PodRequestRepository.Initialize();
            await RadioConnectionRepository.Initialize();

            var count = await (await MedicationRepository.ForQuery()).CountAsync();
            if (count > 0)
                return;

            var med1 = await MedicationRepository.Create(
                new Medication { Hormone = HormoneType.Insulin, Name = "Novorapid U100", UnitName = "Units", UnitNameShort = "U", UnitsPerMilliliter = 100m, ProfileCode = "NRAP1" });

            var med2 = await MedicationRepository.Create(
                new Medication { Hormone = HormoneType.Insulin, Name = "Fiasp U100", UnitName = "Units", UnitNameShort = "U", UnitsPerMilliliter = 100m, ProfileCode = "URAP1" });

#if DEBUG
            var localUser = await UserRepository.Create(
            new User { Name = "TestUser", DateOfBirth = DateTimeOffset.UtcNow.AddYears(-20).AddDays(150), ManagedRemotely = false });

            var remoteUser = await UserRepository.Create(
                new User { Name = "RemoteTestUser", DateOfBirth = DateTimeOffset.UtcNow.AddYears(-8).AddDays(150), ManagedRemotely = true });

            var profile = await UserProfileRepository.Create(new UserProfile
            {
                UserId = localUser.Id.Value,
                MedicationId = med1.Id.Value,
                PodBasalSchedule = new[]
                     {1m, 1m, 1m, 1m, 1m, 1m, 1m, 1m, 1m, 1m, 1m, 1m, 1m, 1m, 1m, 1m, 1m, 1m, 1m, 1m, 1m, 1m, 1m, 1m,
                      1m, 1m, 1m, 1m, 1m, 1m, 1m, 1m, 1m, 1m, 1m, 1m, 1m, 1m, 1m, 1m, 1m, 1m, 1m, 1m, 1m, 1m, 1m, 1m}
            });
#endif
        }
    }
}
