using OmniCore.Model.Interfaces;
using OmniCore.Model.Interfaces.Data;
using OmniCore.Model.Interfaces.Data.Entities;
using OmniCore.Model.Interfaces.Data.Repositories;
using OmniCore.Model.Interfaces.Platform;
using OmniCore.Repository.Sqlite.Entities;
using OmniCore.Repository.Sqlite.Repositories;

namespace OmniCore.Repository.Sqlite
{
    public static class Initializer
    {
        public static ICoreContainer WithSqliteRepositories(this ICoreContainer container)
        {
            return container
                .Many<IPodRepository, PodRepository>()
                .Many<IMedicationRepository, MedicationRepository>()
                .Many<IRadioEventRepository, RadioEventRepository>()
                .Many<IRadioRepository, RadioRepository>()
                .Many<ISignalStrengthRepository, SignalStrengthRepository>()
                .Many<IUserRepository, UserRepository>()
                .Many<IMigrationHistoryRepository, MigrationHistoryRepository>()
                .Many<IRepositoryMigrator, RepositoryMigrator>()
                .One<IRepositoryService, RepositoryService>();
        }
    }
}
