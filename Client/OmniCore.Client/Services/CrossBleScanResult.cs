using OmniCore.Model.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace OmniCore.Client.Services
{
    public class CrossBleScanResult : IRadioPeripheralScanResult
    {
        public IRadioPeripheral RadioPeripheral { get; set; }

        public int Rssi { get; set; }
    }
}
