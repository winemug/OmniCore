using OmniCore.Model.Interfaces.Base;

namespace OmniCore.Model.Interfaces.Services.Facade
{
    public interface IPodRequest : ITask, IServerResolvable
    {
        IPod Pod { get; set; }
    }
}