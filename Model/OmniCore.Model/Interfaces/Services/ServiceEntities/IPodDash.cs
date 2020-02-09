using System.Threading.Tasks;
using OmniCore.Model.Interfaces.Platform.Common;

namespace OmniCore.Model.Interfaces.Platform.Server
{
    public interface IPodDash : IPod
    {
        Task<IPodRequest> Activate();
    }
}