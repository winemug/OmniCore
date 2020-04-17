using OmniCore.Model.Interfaces.Common;

namespace OmniCore.Simulation
{
    public static class Initializer
    {
        public static IContainer<IServiceInstance> WithBleSimulator
            (this IContainer<IServiceInstance> container)
        {
            return container;
        }
    }
}