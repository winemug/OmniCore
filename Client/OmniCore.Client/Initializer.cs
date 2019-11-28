using OmniCore.Client.Services;
using Unity;
using OmniCore.Model.Interfaces.Platform;

namespace OmniCore.Client
{
    public static class Initializer
    {
        public static IUnityContainer WithCrossPlatformBleAdapter(this IUnityContainer container)
        {
            container.RegisterSingleton<IRadioAdapter, CrossBleRadioAdapter>();
            return container;
        }
    }
}
