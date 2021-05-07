﻿using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using OmniCore.Client.Annotations;
using OmniCore.Model.Interfaces.Services;

namespace OmniCore.Client.Models
{
    public class RadioModel : INotifyPropertyChanged
    {
        public readonly IErosRadio Radio;

        public RadioModel(IErosRadio radio)
        {
            Radio = radio;

            radio.Name.Subscribe(s => Name = s);
            radio.Rssi.Subscribe(rssi => Rssi = $"{rssi} db");
            radio.DiscoveryState.Subscribe(state => DiscoveryState = state.ToString());
            radio.ConnectionState.Subscribe(state => ConnectionState = state.ToString());
        }

        public string Name { get; set; }

        public string MacAddress => Radio.Address;
        public string Rssi { get; set; }
        public string DiscoveryState { get; set; }
        public string ConnectionState { get; set; }
        public string Activity { get; set; }

        public bool IsChecked { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}