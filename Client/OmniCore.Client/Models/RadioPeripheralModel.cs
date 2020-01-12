using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using OmniCore.Client.Annotations;
using OmniCore.Model.Extensions;
using OmniCore.Model.Interfaces.Platform.Common;

namespace OmniCore.Client.Models
{
    public class RadioPeripheralModel : INotifyPropertyChanged
    {
        private readonly IRadioPeripheral Peripheral;
        public string Name { get; set; }
        public string MacAddress => Peripheral.PeripheralUuid.AsMacAddress();
        public string Rssi { get; set; }
        public string DiscoveryState { get; set; }
        public string ConnectionState { get; set; }
        public bool IsChecked { get; set; }

        public RadioPeripheralModel(IRadioPeripheral peripheral)
        {
            Peripheral = peripheral;
            Peripheral.Name.Subscribe(s => Name = s);
            Peripheral.Rssi.Subscribe(rssi => Rssi = $"{rssi} db");
            Peripheral.State.Subscribe(state => DiscoveryState = state.ToString());
            Peripheral.ConnectionState.Subscribe(state => ConnectionState = state.ToString());
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
