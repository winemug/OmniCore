using OmniCore.Repository.Sqlite;
using Unity;

namespace OmniCore.Data
{
    public static class Initializer
    {
        public static IUnityContainer WithDefaultDataServices(this IUnityContainer container)
        {
            container.WithSqliteRepositories();
            
            return container;
        }
    }
}
