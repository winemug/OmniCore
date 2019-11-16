using System;
using System.Collections.Generic;
using System.Text;

namespace OmniCore.Model.Interfaces.Platform
{
    public interface IRadioPeripheralLease : IDisposable
    {
        IRadioPeripheral Peripheral { get; }
    }
}
