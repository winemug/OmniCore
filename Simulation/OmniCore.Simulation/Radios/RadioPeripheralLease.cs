using System;
using System.Collections.Generic;
using System.Text;
using OmniCore.Model.Interfaces.Platform;

namespace OmniCore.Simulation.Radios
{
    public class RadioPeripheralLease : IRadioPeripheralLease
    {
        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public IRadioPeripheral Peripheral { get; }
    }
}
