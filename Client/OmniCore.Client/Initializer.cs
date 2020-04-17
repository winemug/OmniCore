using OmniCore.Client.Platform;
using OmniCore.Model.Interfaces.Client;
using OmniCore.Model.Interfaces.Common;
using OmniCore.Model.Interfaces.Services.Internal;

namespace OmniCore.Client
{
    public static class Initializer
    {
        public static IContainer<IServiceInstance> WithCrossBleRadioAdapter
            (this IContainer<IServiceInstance> container)
        {
            return container
                .One<IBlePeripheralAdapter, BlePeripheralAdapter>()
                .Many<IBlePeripheral, BlePeripheral>()
                .Many<IBlePeripheralConnection, BlePeripheralConnection>();
        }

        public static IContainer<IClientInstance> WithXamarinFormsClient
            (this IContainer<IClientInstance> container)
        {
            return container
                .One<IClient, XamarinClient>();
        }
    }
}