using OmniCore.Model.Interfaces;
using OmniCore.Model.Interfaces.Platform;
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
        
        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
