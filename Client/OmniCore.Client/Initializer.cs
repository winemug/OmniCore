using OmniCore.Client.Platform;
using OmniCore.Client.ViewModels.Base;
using OmniCore.Client.ViewModels.Testing;
using OmniCore.Client.Views.Base;
using OmniCore.Client.Views.Main;
using OmniCore.Client.Views.Testing;
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

            container.RegisterType<RadiosViewModel>();
            container.RegisterType<RadiosView>();

            container.RegisterType<RadioDiagnosticsViewModel>();
            container.RegisterType<RadioDiagnosticsView>();

            return container;
        }
    }
}
