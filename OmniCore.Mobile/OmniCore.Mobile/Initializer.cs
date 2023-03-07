using OmniCore.Mobile.Services;
using OmniCore.Mobile.ViewModels;
using OmniCore.Mobile.Views;
using OmniCore.Services.Interfaces;
using Unity;
using Unity.Lifetime;
using Xamarin.Forms;

namespace OmniCore.Mobile
{
    public static class Initializer
    {
        public static void RegisterTypesForMobileApp(IUnityContainer container)
        {
            // services
            OmniCore.Services.Initializer.RegisterTypesForServices(container);

            // platform specific instances
            container.RegisterInstance(DependencyService.Resolve<IPlatformInfo>());
            container.RegisterInstance(DependencyService.Resolve<IForegroundServiceHelper>());


            // mobile
            container.RegisterType<NavigationService>(new ContainerControlledLifetimeManager());
            var navigationService = container.Resolve<NavigationService>();
            
            // navigation mappings
            navigationService.Map<StartPage, StartViewModel>();
            navigationService.Map<PlatformConfigurationPage, PlatformConfigurationViewModel>();
            navigationService.Map<AccountLoginPage, AccountLoginViewModel>();
            navigationService.Map<ClientRegistrationPage, ClientRegistrationViewModel>();
            navigationService.Map<BluetoothTestPage, BluetoothTestViewModel>();
        }
    }
}