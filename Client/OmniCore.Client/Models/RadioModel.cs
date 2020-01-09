using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reactive.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using OmniCore.Client.Annotations;
using OmniCore.Model.Extensions;
using OmniCore.Model.Interfaces.Common;

namespace OmniCore.Client.Models
{
    public class RadioModel : INotifyPropertyChanged
    {

        public string Name { get; set; }
        public string MacAddress { get; set; }
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
            var gb = radio.Peripheral.PeripheralUuid.ToByteArray();
            MacAddress = $"{gb[10]:X2}:{gb[11]:X2}:{gb[12]:X2}:{gb[13]:X2}:{gb[14]:X2}:{gb[15]:X2}";

        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
