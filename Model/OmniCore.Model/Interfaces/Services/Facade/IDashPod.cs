using System.Threading.Tasks;

namespace OmniCore.Model.Interfaces.Services.Facade
{
    public interface IDashPod : IPod
    {
        Task<IPodRequest> Activate();
    }
}