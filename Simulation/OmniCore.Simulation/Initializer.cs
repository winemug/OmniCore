using System;
using OmniCore.Model.Interfaces;
using OmniCore.Model.Interfaces.Platform;
using OmniCore.Simulation.Radios;

namespace OmniCore.Simulation
{
    public static class Initializer
    {
        public static ICoreContainer WithBleSimulator(this ICoreContainer container)
        {
            return container.Many<IRadioAdapter, RadioAdapter>();
        }
    }
}
