using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reactive.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using OmniCore.Client.Annotations;
using OmniCore.Model.Extensions;
using OmniCore.Model.Interfaces.Platform.Common;

namespace OmniCore.Client.Models
{
    public class RadioModel : INotifyPropertyChanged
    {

        public string Name { get; set; }

        public string MacAddress => Radio.Peripheral.PeripheralUuid.AsMacAddress();

        public string Rssi { get; set; }
        public string DiscoveryState { get; set; }
        public string ConnectionState { get; set; }
        public string Activity { get; set; }

        public readonly IRadio Radio;
        public RadioModel(IRadio radio)
        {
            Radio = radio;

            radio.Peripheral.Name.Subscribe(s => Name = s);
            radio.Peripheral.Rssi.Subscribe(rssi => Rssi = $"{rssi} db");
            radio.Peripheral.State.Subscribe(state => DiscoveryState = state.ToString());
            radio.Peripheral.ConnectionState.Subscribe(state => ConnectionState = state.ToString());
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
