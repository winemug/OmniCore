using System.ComponentModel;
using OmniCore.Client.Interfaces;
using OmniCore.Client.Platform;
using OmniCore.Client.ViewModels;
using OmniCore.Client.Views;
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

            container.RegisterType<ShellViewModel>();
            container.RegisterType<ShellView>();

            container.RegisterType<EmptyViewModel>();
            container.RegisterType<EmptyView>();

            return container;
        }
    }
}
