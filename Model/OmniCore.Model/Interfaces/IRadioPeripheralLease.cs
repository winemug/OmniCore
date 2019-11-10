using System;
using System.Collections.Generic;
using System.Text;

namespace OmniCore.Model.Interfaces
{
    public interface IRadioPeripheralLease : IDisposable
    {
        IRadioPeripheral Peripheral { get; }
    }
}
