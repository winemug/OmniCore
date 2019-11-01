using System;
using System.Collections.Generic;
using System.Text;

namespace OmniCore.Model.Interfaces
{
    public interface IRadioPeripheral
    {
        int Rssi { get; }
        Guid PeripheralId { get; }
        string PeripheralName { get; }
    }
}
