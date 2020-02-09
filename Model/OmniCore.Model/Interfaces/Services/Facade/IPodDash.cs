using System.Threading.Tasks;

namespace OmniCore.Model.Interfaces.Services.Facade
{
    public interface IPodDash : IPod
    {
        Task<IPodRequest> Activate();
    }
}