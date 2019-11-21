using OmniCore.Model.Interfaces.Repositories;
using OmniCore.Model.Interfaces.Services;
using OmniCore.Repository.Sqlite.Entities;
using OmniCore.Repository.Sqlite.Repositories;
using Unity;

namespace OmniCore.Repository.Sqlite
{
    public static class Initializer
    {
        public static void RegisterTypes(IUnityContainer container)
        {
            container.RegisterType<IMedicationRepository, MedicationRepository>();
            container.RegisterType<IUserRepository, UserRepository>();

            container.RegisterType<IRepositoryService, RepositoryService>();
        }
    }
}
