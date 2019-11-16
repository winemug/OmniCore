using System;
using System.Collections.Generic;
using System.Text;

namespace OmniCore.Model.Interfaces.Platform
{
    public interface IRadioPeripheralScanResult
    {
        Guid Id { get; }
        string Name { get; }
        int Rssi { get; }
    }
}
