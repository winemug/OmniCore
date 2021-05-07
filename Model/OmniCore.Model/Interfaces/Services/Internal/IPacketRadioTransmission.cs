using System;
using System.Collections.Generic;
using System.Text;
using OmniCore.Model.Enumerations;

namespace OmniCore.Model.Interfaces.Services.Internal
{
    public interface IPacketRadioTransmission
    {
        RadioTransmissionSequence Sequence { get; }
        byte[] Tx { get; set; }
        TimeSpan TxTimeout { get; set; }

        byte[] Rx { get; set; }
        TimeSpan RxTimeout { get; set; }

        int? Rssi { get; set; }
        int Channel { get; set; }
        TransmissionPower? PowerOverride { get; set; }
    }
}
