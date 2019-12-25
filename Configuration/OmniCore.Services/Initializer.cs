using OmniCore.Model.Interfaces.Services;
using Unity;

namespace OmniCore.Services
{
    public static class Initializer
    {
        public static IUnityContainer WithDefaultServices(this IUnityContainer container)
        {
            container.RegisterSingleton<ICoreDataServices, CoreDataServices>();
            container.RegisterSingleton<ICoreIntegrationServices, CoreIntegrationServices>();
            container.RegisterSingleton<ICoreAutomationServices, CoreAutomationServices>();
           
            return container;
        }
    }
}
