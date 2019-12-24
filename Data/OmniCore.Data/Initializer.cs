using OmniCore.Model.Interfaces.Services;
using OmniCore.Repository.Sqlite;
using Unity;

namespace OmniCore.Data
{
    public static class Initializer
    {
        public static IUnityContainer WithDefaultDataServices(this IUnityContainer container)
        {
            container.RegisterSingleton<ICoreDataServices, CoreDataServices>();
            container.WithSqliteRepositories();
            
            return container;
        }
    }
}
