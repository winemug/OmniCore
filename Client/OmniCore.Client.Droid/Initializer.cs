using OmniCore.Client.Droid.Services;
using OmniCore.Model.Interfaces.Services;
using Unity;

namespace OmniCore.Client.Droid
{
    public static class Initializer
    {
        public static IUnityContainer OnAndroidPlatform(this IUnityContainer container)
        {
            container.RegisterType<IApplicationService, ApplicationService>();
            return container;
        }
    }
}