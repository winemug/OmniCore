using OmniCore.Model.Entities;
using OmniCore.Model.Enumerations;

namespace OmniCore.Model.Interfaces.Services.Internal
{
    public interface IErosPodResponseMessage : IPodResponse
    {
        void ParseResponse(byte[] responseData);
        bool IsValid { get; }
        PodProgress? Progress { get; }
        bool? Faulted { get; }
        uint RadioAddress { get; }
        PodResponseFault FaultResponse { get; }
        PodResponseRadio RadioResponse { get; }
        PodResponseStatus StatusResponse { get; }
        PodResponseVersion VersionResponse { get; }
    }
}