using System;
using OmniCore.Model.Interfaces.Platform;
using OmniCore.Simulation.Radios;
using Unity;

namespace OmniCore.Simulation
{
    public static class Initializer
    {
        public static IUnityContainer WithBleSimulator(this IUnityContainer container)
        {
            container.RegisterSingleton<IRadioAdapter, RadioAdapter>();
            return container;
        }
    }
}
