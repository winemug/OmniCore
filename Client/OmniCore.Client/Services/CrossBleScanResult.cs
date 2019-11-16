using OmniCore.Model.Interfaces;
using OmniCore.Model.Interfaces.Platform;
using System;
using System.Collections.Generic;
using System.Text;

namespace OmniCore.Client.Services
{
    public class CrossBleScanResult : IRadioPeripheralScanResult
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public int Rssi { get; set; }

    }
}
