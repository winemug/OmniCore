using System;
using OmniCore.Model.Enumerations;

namespace OmniCore.Model.Interfaces.Services.Internal
{
    public interface IMessageRadioTransmission
    {
        byte[] Tx { get; set; }
        TimeSpan TxTimeout { get; set; }

        byte[] Rx { get; set; }
        TimeSpan RxTimeout { get; set; }

        int? RssiAverage { get; set; }
        int Channel { get; set; }
        TransmissionPower? PowerOverride { get; set; }
    }
}