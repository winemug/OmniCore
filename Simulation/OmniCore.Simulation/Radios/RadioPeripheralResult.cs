using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using OmniCore.Model.Interfaces;

namespace OmniCore.Simulation.Radios
{
    public class RadioPeripheralResult : IRadioPeripheralResult
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public IRadioPeripheral Peripheral { get; }
        public int? Rssi { get; set; }
    }
}
