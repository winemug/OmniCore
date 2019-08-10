using System;
using System.Collections.Generic;
using System.Text;
using OmniCore.Model.Interfaces;
using Plugin.BluetoothLE;

namespace OmniCore.Mobile.Services
{
    public class CrossBleRadioPeripheral : IRadioPeripheral
    {
        private IDevice _bleDevice;
        public CrossBleRadioPeripheral(IDevice bleDevice, int rssi)
        {
            _bleDevice = bleDevice;
            Rssi = rssi;
        }

        public int Rssi { get; }

        public Guid PeripheralId => _bleDevice.Uuid;

        public string PeripheralName => _bleDevice.Name;
    }
}
