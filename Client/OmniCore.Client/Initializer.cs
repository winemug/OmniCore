using OmniCore.Client.Platform;
using OmniCore.Client.ViewModels.Base;
using OmniCore.Client.ViewModels.Home;
using OmniCore.Client.Views.Base;
using OmniCore.Client.Views.Main;
using OmniCore.Client.Views.Home;
using OmniCore.Model.Interfaces.Platform;
using Unity;

namespace OmniCore.Client
{
    public static class Initializer
    {
        public static IUnityContainer WithCrossBleAdapter(this IUnityContainer container)
        {
            container.RegisterSingleton<IRadioAdapter, CrossBleRadioAdapter>();
            return container;
        }

        public static IUnityContainer WithXamarinForms(this IUnityContainer container)
        {
            container.RegisterType<UnityRouteFactory>();
            
            container.RegisterType<ShellViewModel>();
            container.RegisterType<ShellView>();

            container.RegisterType<EmptyViewModel>();
            container.RegisterType<EmptyView>();

            container.RegisterType<RadiosViewModel>();
            container.RegisterType<RadiosView>();

            container.RegisterType<RadioDetailViewModel>();
            container.RegisterType<RadioDetailView>();

            return container;
        }
    }
}
