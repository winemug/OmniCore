using OmniCore.Model.Entities;
using OmniCore.Model.Enumerations;

namespace OmniCore.Model.Interfaces.Services.Internal
{
    public interface IErosPodRequestMessage
    {
        IErosPod ErosPod { get; }
        IErosPodRequestMessage WithPod(IErosPod pod);
        IErosPodRequestMessage WithMessageAddress(uint messageAddress);
        IErosPodRequestMessage WithAllowAddressOverride();
        IErosPodRequestMessage WithTransmissionPower(TransmissionPower transmissionPower);
        IErosPodRequestMessage WithPairRequest(uint radioAddress);
        IErosPodRequestMessage WithMessageSequence(int messageNo);
        IErosPodRequestMessage WithCriticalFollowup();
        IErosPodRequestMessage WithStatusRequest(StatusRequestType requestType);
        IErosPodRequestMessage WithAcquireRequest();
        
        byte[] Data { get; }
        uint MessageAddress { get; }
        bool AllowAddressOverride { get; }
    }
}