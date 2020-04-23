using OmniCore.Model.Interfaces;

namespace OmniCore.Simulation
{
    public static class Initializer
    {
        public static IContainer WithBleSimulator
            (this IContainer container)
        {
            return container;
        }
    }
}