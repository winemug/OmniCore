using System;
using System.Threading;
using System.Threading.Tasks;
using OmniCore.Model.Entities;

namespace OmniCore.Model.Interfaces.Services.Internal
{
    public interface IRadioConnection
    {
        Task Configure(RadioOptions options, CancellationToken cancellationToken);
        Task Transceive(IPacketRadioTransmission packetRadioTransmission, CancellationToken cancellationToken);
        Task Transceive(IMessageRadioTransmission messageRadioTransmission, CancellationToken cancellationToken);
        Task FlashLights(CancellationToken cancellationToken);
    }
}