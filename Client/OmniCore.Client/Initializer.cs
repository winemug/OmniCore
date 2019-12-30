using OmniCore.Client.Platform;
using OmniCore.Client.ViewModels.Base;
using OmniCore.Client.ViewModels.Home;
using OmniCore.Client.ViewModels.Wizards;
using OmniCore.Client.Views.Base;
using OmniCore.Client.Views.Main;
using OmniCore.Client.Views.Home;
using OmniCore.Client.Views.Wizards.NewPod;
using OmniCore.Model.Interfaces;
using OmniCore.Model.Interfaces.Services;

namespace OmniCore.Client
{
    public static class Initializer
    {
        public static ICoreContainer WithCrossBleAdapter(this ICoreContainer container)
        {
            return container.One<IRadioAdapter, CrossBleRadioAdapter>();
        }

        public static ICoreContainer WithXamarinForms(this ICoreContainer container)
        {
            return container
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
