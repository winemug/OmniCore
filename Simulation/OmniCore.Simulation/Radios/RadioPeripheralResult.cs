using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using OmniCore.Model.Interfaces;

namespace OmniCore.Simulation.Radios
{
    public class RadioPeripheralResult : IRadioPeripheralResult
    {
#pragma warning disable CS0067 // The event 'RadioPeripheralResult.PropertyChanged' is never used
        public event PropertyChangedEventHandler PropertyChanged;
#pragma warning restore CS0067 // The event 'RadioPeripheralResult.PropertyChanged' is never used
        public IRadioPeripheral Peripheral { get; }
        public int? Rssi { get; set; }
    }
}
