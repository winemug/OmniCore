using OmniCore.Model.Interfaces.Common;

namespace OmniCore.Simulation
{
    public static class Initializer
    {
        public static ICoreContainer<IServerResolvable> WithBleSimulator
            (this ICoreContainer<IServerResolvable> container)
        {
            return container;
        }
    }
}