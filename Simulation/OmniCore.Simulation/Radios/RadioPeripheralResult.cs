using System;
using System.Collections.Generic;
using System.Text;
using OmniCore.Model.Interfaces.Platform;

namespace OmniCore.Simulation.Radios
{
    public class RadioPeripheralResult : IRadioPeripheralResult
    {
        public Guid Uuid { get; }
        public string Name { get; }
        public int Rssi { get; }
    }
}
