using OmniCore.Model.Interfaces.Data.Repositories;
using OmniCore.Model.Interfaces.Platform.Common;
using OmniCore.Model.Interfaces.Platform.Common.Data;
using OmniCore.Model.Interfaces.Platform.Common.Data.Entities;
using OmniCore.Model.Interfaces.Platform.Common.Data.Repositories;
using OmniCore.Repository.Sqlite.Entities;
using OmniCore.Repository.Sqlite.Repositories;

namespace OmniCore.Repository.Sqlite
{
    public static class Initializer
    {
        public static ICoreContainer<IServerResolvable> WithSqliteRepositories
            (this ICoreContainer<IServerResolvable> container)
        {
            return container
                .Many<IPodRepository, PodRepository>()
                .Many<IMedicationRepository, MedicationRepository>()
                .Many<IRadioEventRepository, RadioEventRepository>()
                .Many<IRadioRepository, RadioRepository>()
                .Many<ISignalStrengthRepository, SignalStrengthRepository>()
                .Many<IUserRepository, UserRepository>()
                .Many<IMigrationHistoryRepository, MigrationHistoryRepository>()
                .Many<IPodRequestRepository, PodRequestRepository>()
                .Many<IMedicationDeliveryRepository, MedicationDeliveryRepository>()
                .Many<IRepositoryMigrator, RepositoryMigrator>()
                .One<IRepositoryService, RepositoryService>();
        }
    }
}
