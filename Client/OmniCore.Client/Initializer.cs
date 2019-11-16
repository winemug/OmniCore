using OmniCore.Eros;
using OmniCore.Client.Services;
using OmniCore.Model.Interfaces;
using Unity;
using OmniCore.Model.Interfaces.Platform;

namespace OmniCore.Client
{
    public static class Initializer
    {
        public static void RegisterTypes(IUnityContainer container)
        {
            container.RegisterSingleton<IRadioAdapter, CrossBleRadioAdapter>();
            OmniCore.Eros.Initializer.RegisterTypes(container);
        }
    }
}
