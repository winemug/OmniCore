using OmniCore.Model.Interfaces;
using OmniCore.Model.Interfaces.Platform;
using System;
using System.Collections.Generic;
using System.Text;

namespace OmniCore.Client.Platform
{
    public class CrossBlePeripheralLease : IRadioPeripheralLease
    {
        public IRadioPeripheral Peripheral { get => CrossBlePeripheral; }
        public CrossBleRadioPeripheral CrossBlePeripheral { get; set; }
        public CrossBlePeripheralLease(CrossBleRadioPeripheral peripheral)
        {
            CrossBlePeripheral = peripheral;
        }

        public void Dispose()
        {
            CrossBlePeripheral?.Release();
            CrossBlePeripheral = null;
        }
    }
}
