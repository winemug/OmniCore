using OmniCore.Client.Platform;
using OmniCore.Client.ViewModels.Base;
using OmniCore.Client.ViewModels.Home;
using OmniCore.Client.ViewModels.Wizards;
using OmniCore.Client.Views.Base;
using OmniCore.Client.Views.Main;
using OmniCore.Client.Views.Home;
using OmniCore.Client.Views.Wizards.NewPod;
using OmniCore.Model.Interfaces;
using OmniCore.Model.Interfaces.Platform;

namespace OmniCore.Client
{
    public static class Initializer
    {
        public static ICoreContainer<IServerResolvable> WithCrossBleRadioAdapter
            (this ICoreContainer<IServerResolvable> container)
        {
            return container.One<IRadioAdapter, CrossBleRadioAdapter>();
        }

        public static ICoreContainer<IClientResolvable> WithXamarinForms
            (this ICoreContainer<IClientResolvable> container)
        {
            return container
                .One<XamarinApp>()
                .One<UnityRouteFactory>()
                .Many<ShellViewModel>()
                .Many<ShellView>()
                .Many<EmptyViewModel>()
                .Many<EmptyView>()
                .Many<PodsViewModel>()
                .Many<PodsView>()
                .Many<PodWizardViewModel>()
                .Many<PodWizardMainView>()
                .Many<RadiosViewModel>()
                .Many<RadiosView>()
                .Many<RadioDetailViewModel>()
                .Many<RadioDetailView>();
        }
    }
}
