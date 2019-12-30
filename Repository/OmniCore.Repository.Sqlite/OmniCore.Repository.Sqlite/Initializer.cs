using OmniCore.Model.Interfaces;
using OmniCore.Model.Interfaces.Data;
using OmniCore.Model.Interfaces.Data.Repositories;
using OmniCore.Model.Interfaces.Services;
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
                .One<IRepositoryService, RepositoryService>();
        }
    }
}
