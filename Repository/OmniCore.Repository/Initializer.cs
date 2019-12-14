using OmniCore.Model.Interfaces.Repositories;
using OmniCore.Model.Interfaces.Services;
using Unity;

namespace OmniCore.Repository
{
    public static class Initializer
    {
        public static IUnityContainer WithSqliteRepository(this IUnityContainer container)
        {
            container.RegisterSingleton<IRepositoryService, RepositoryService>();
            return container;
        }
    }
}
