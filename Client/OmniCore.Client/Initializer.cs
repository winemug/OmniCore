using OmniCore.Client.Platform;
using OmniCore.Model.Interfaces;
using OmniCore.Model.Interfaces.Services.Internal;

namespace OmniCore.Client
{
    public static class Initializer
    {
        public static IContainer WithCrossBleRadioAdapter
            (this IContainer container)
        {
            return container
                .One<IBlePeripheralAdapter, BlePeripheralAdapter>()
                .Many<IBlePeripheral, BlePeripheral>()
                .Many<IBlePeripheralConnection, BlePeripheralConnection>();
        }

        public static IContainer WithXamarinFormsClient
            (this IContainer container)
        {
            return container
                .One<IClient, XamarinClient>();
        }
    }
}