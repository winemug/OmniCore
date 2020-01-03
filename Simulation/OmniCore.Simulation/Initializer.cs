using System;
using OmniCore.Model.Interfaces;
using OmniCore.Simulation.Radios;

namespace OmniCore.Simulation
{
    public static class Initializer
    {
        public static ICoreContainer<IServerResolvable> WithBleSimulator
            (this ICoreContainer<IServerResolvable> container)
        {
            return container.Many<IRadioAdapter, RadioAdapter>();
        }
    }
}
