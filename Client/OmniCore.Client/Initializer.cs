using OmniCore.Client.Platform;
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