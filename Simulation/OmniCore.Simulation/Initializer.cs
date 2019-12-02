using System;
using Unity;

namespace OmniCore.Simulation
{
    public static class Initializer
    {
        public static IUnityContainer WithBleSimulator(this IUnityContainer container)
        {
            return container;
        }
    }
}
