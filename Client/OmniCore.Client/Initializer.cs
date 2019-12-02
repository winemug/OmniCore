using OmniCore.Client.Services;
using Unity;
using OmniCore.Model.Interfaces.Platform;

namespace OmniCore.Client
{
    public static class Initializer
    {
        public static IUnityContainer WithCrossBleAdapter(this IUnityContainer container)
        {
            container.RegisterSingleton<IRadioAdapter, CrossBleRadioAdapter>();
            return container;
        }

        public static IUnityContainer AsXamarinApplication(this IUnityContainer container)
        {
            container.RegisterSingleton<IUserInterface, XamarinApp>();
            return container;
        }
    }
}
