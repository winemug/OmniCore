using System.Threading.Tasks;

namespace OmniCore.Model.Interfaces.Services
{
    public interface IDashPod : IPod
    {
        Task<IPodTask> Activate();
    }
}