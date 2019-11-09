using System;
using System.Collections.Generic;
using System.Text;

namespace OmniCore.Model.Interfaces
{
    public interface IRadioPeripheralScanResult
    {
        IRadioPeripheral RadioPeripheral { get; }
        int Rssi { get; }
    }
}
