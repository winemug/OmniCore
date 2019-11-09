using System;
using System.Collections.Generic;
using System.Reactive.Threading.Tasks;
using System.Text;
using System.Threading.Tasks;
using OmniCore.Model.Interfaces;
using Plugin.BluetoothLE;

namespace OmniCore.Mobile.Services
{
    public class CrossBleRadioPeripheral : IRadioPeripheral
    {
        private IDevice BleDevice;

        public CrossBleRadioPeripheral(IDevice bleDevice)
        {
            BleDevice = bleDevice;
        }

        public Guid PeripheralId => BleDevice.Uuid;

        public string PeripheralName => BleDevice.Name;

        public async Task<bool> IsConnected()
        {
            var connected = false;
            if (BleDevice.IsConnected())
            {

            }
            return connected;
        }
    }
}
