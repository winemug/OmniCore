using System;
using System.Collections.Generic;
using System.Text;

namespace OmniCore.Model.Interfaces.Platform
{
    public interface IRadioPeripheralResult
    {
        Guid Uuid { get; }
        string Name { get; }
        int Rssi { get; }
    }
}
