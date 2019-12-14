using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace OmniCore.Model.Interfaces.Platform
{
    public interface IRadioPeripheralResult : INotifyPropertyChanged
    {
        IRadioPeripheral Peripheral { get; }
        int? Rssi { get; set; }
    }
}
