using OmniCore.Model.Enumerations;

namespace OmniCore.Model.Interfaces.Services.Requests
{
    public interface IPodStatusRequest : IPodRequest
    {
        StatusRequestType RequestType { get; set; }
        IPodStatusRequest WithUpdateStatus();
    }
}