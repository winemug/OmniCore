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

        private static bool IsInitialized = false;
        private static RepositoryProvider InternalInstance = new RepositoryProvider();


        public static RepositoryProvider Instance
        {
            get
            {
                if (!IsInitialized)
                {
                    lock (InternalInstance)
                    {
                        if (!IsInitialized)
                        {
                            Task.Run(async () => await Instance.Initialize()).Wait();
                            IsInitialized = true;
                        }
                    }
                }
                return InternalInstance;
            }
        }

        private SQLiteAsyncConnection Connection;
        public string DatabasePath;

        public MedicationRepository MedicationRepository { get; private set; }
        public UserRepository UserRepository { get; private set; }
        public UserProfileRepository UserProfileRepository { get; private set; }
        public RadioRepository RadioRepository { get; private set; }
        public PodRepository PodRepository { get; private set; }
        public PodRequestRepository PodRequestRepository { get; private set; }
        public RadioConnectionRepository RadioConnectionRepository { get; private set; }

    private async Task Initialize()
        {
            DatabasePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "omnicore.db3");
#if DEBUG
            if (File.Exists(DatabasePath))
                File.Delete(DatabasePath);
#endif
            Connection = new SQLiteAsyncConnection(DatabasePath, SQLiteOpenFlags.Create | SQLiteOpenFlags.ReadWrite);

            //TODO: lame repo inits, to be ioc'ed later
            MedicationRepository = new MedicationRepository(Connection);
            await MedicationRepository.Initialize();

            UserRepository = new UserRepository(Connection);
            await UserRepository.Initialize();

            UserProfileRepository = new UserProfileRepository(Connection);
            await UserProfileRepository.Initialize();

            RadioRepository = new RadioRepository(Connection);
            await RadioRepository.Initialize();

            PodRepository = new PodRepository(Connection);
            await PodRepository.Initialize();

            PodRequestRepository = new PodRequestRepository(Connection);
            await PodRequestRepository.Initialize();

            RadioConnectionRepository = new RadioConnectionRepository(Connection);
            await RadioConnectionRepository.Initialize();

            var med1 = await MedicationRepository.Create(
                new Medication { Hormone = HormoneType.Insulin, Name = "Novorapid U100", UnitName = "Units", UnitNameShort = "U", UnitsPerMilliliter = 100m, ProfileCode = "NRAP1" });

            var med2 = await MedicationRepository.Create(
                new Medication { Hormone = HormoneType.Insulin, Name = "Fiasp U100", UnitName = "Units", UnitNameShort = "U", UnitsPerMilliliter = 100m, ProfileCode = "URAP1" });

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

        }
    }
}
