using OmniCore.Model.Interfaces;
using OmniCore.Model.Interfaces.Platform;
using System;
using System.Collections.Generic;
using System.Text;

namespace OmniCore.Client.Platform
{
    public class CrossBleResult : IRadioPeripheralResult
    {
        public Guid Uuid { get; set; }
        public string Name { get; set; }
        public int Rssi { get; set; }

    }
}
