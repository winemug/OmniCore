using OmniCore.Model.Interfaces.Services;
using Unity;

namespace OmniCore.Services
{
    public static class Initializer
    {
        public static IUnityContainer WithDefaultServiceProviders(this IUnityContainer container)
        {
            container.RegisterType<IUserService, UserService>();
            container.RegisterType<IPodService, PodService>();
            container.RegisterType<IRadioService, RadioService>();

            container.RegisterType<ICoreServices, LocalServices>();
            container.RegisterType<ICoreServicesProvider, CoreServicesProvider>();
            
            return container;
        }
    }
}
