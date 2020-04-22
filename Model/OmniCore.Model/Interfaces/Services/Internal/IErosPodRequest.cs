using OmniCore.Model.Entities;
using OmniCore.Model.Enumerations;

namespace OmniCore.Model.Interfaces.Services.Internal
{
    public interface IErosPodRequest : IPodRequest
    {
        IErosPod ErosPod { get; }
        IErosPodRequest WithPod(IErosPod pod);
        IErosPodRequest WithEntity(PodRequestEntity entity);
        IErosPodRequest WithMessageAddress(uint messageAddress);
        IErosPodRequest WithMessageSequence(int messageSequence);
        IErosPodRequest WithCriticalFollowup();
        IErosPodRequest WithAllowAddressOverride();
        IErosPodRequest WithTransmissionPower(TransmissionPower transmissionPower);
        IErosPodRequest WithStatusRequest(StatusRequestType requestType);
        IErosPodRequest WithAcquireRequest();
        IErosPodRequest WithPairRequest(uint radioAddress);
        
        byte[] Message { get; }
        uint MessageAddress { get; }
        bool AllowAddressOverride { get; }
    }
}