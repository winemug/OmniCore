using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using OmniCore.Model.Entities;
using OmniCore.Model.Interfaces.Platform.Common;

namespace OmniCore.Model.Interfaces.Platform.Common
{
    public interface IPodRequest : ITask, IServerResolvable
    {
        PodRequestEntity Entity { get; set; }
        IPod Pod { get; set; }
    }
}