using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using OmniCore.Model.Interfaces.Platform.Common;
using OmniCore.Model.Interfaces.Platform.Common.Data.Entities;

namespace OmniCore.Model.Interfaces.Platform.Common
{
    public interface IPodRequest : ITask, IServerResolvable
    {
        IPodRequestEntity Entity { get; set; }
        IPod Pod { get; set; }
    }
}