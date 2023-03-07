using OmniCore.Mobile.Services;
using OmniCore.Mobile.ViewModels;
using OmniCore.Mobile.Views;
using Unity;
using Unity.Lifetime;

namespace OmniCore.Mobile
{
    public static class Initializer
    {
        public static void RegisterTypes(IUnityContainer container)
        {
            OmniCore.Services.Initializer.RegisterTypes(container);


            container.RegisterType<NavigationService>(new ContainerControlledLifetimeManager());

            var navigationService = container.Resolve<NavigationService>();
            navigationService.Map<StartPage, StartViewModel>();
            navigationService.Map<PlatformConfigurationPage, PlatformConfigurationViewModel>();
            navigationService.Map<AccountLoginPage, AccountLoginViewModel>();
            navigationService.Map<ClientRegistrationPage, ClientRegistrationViewModel>();
            navigationService.Map<BluetoothTestPage, BluetoothTestViewModel>();
            //navigationService.Register<TwoFactorAuthenticationPage>();
        }
    }
}