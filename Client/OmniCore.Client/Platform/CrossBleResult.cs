using OmniCore.Model.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using OmniCore.Client.Annotations;

namespace OmniCore.Client.Platform
{
    public class CrossBleResult : IRadioPeripheralResult
    {
        public IRadioPeripheral Peripheral { get; set; }
        
        public int? Rssi { get; set; }
        
#pragma warning disable CS0067
        public event PropertyChangedEventHandler PropertyChanged;
#pragma warning restore CS0067
    }
}
