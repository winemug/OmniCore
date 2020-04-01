using OmniCore.Client.Platform;
using OmniCore.Client.ViewModels.Base;
using OmniCore.Client.ViewModels.Home;
using OmniCore.Client.ViewModels.Wizards;
using OmniCore.Client.Views.Base;
using OmniCore.Client.Views.Main;
using OmniCore.Client.Views.Home;
using OmniCore.Client.Views.Wizards.NewPod;
using OmniCore.Model.Interfaces.Client;
using OmniCore.Model.Interfaces.Common;
using OmniCore.Model.Interfaces.Services.Internal;

namespace OmniCore.Client
{
    public static class Initializer
    {
        public static ICoreContainer<IServerResolvable> WithCrossBleRadioAdapter
            (this ICoreContainer<IServerResolvable> container)
        {
            return container
                .One<IBlePeripheralAdapter, BlePeripheralAdapter>()
                .Many<IBlePeripheral, BlePeripheral>()
                .Many<IBlePeripheralConnection, BlePeripheralConnection>();
        }

        public static ICoreContainer<IClientResolvable> WithXamarinForms
            (this ICoreContainer<IClientResolvable> container)
        {
            return container
                .One<IViewPresenter, ViewPresenter>()
                .One<XamarinApp>();
        }
    }
}
