using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using OmniCore.Model.Interfaces.Common;
using OmniCore.Model.Interfaces.Common.Data.Entities;

namespace OmniCore.Model.Interfaces.Common
{
    public interface IPodRequest : ITask, INotifyPropertyChanged, IServerResolvable
    {
        IPodRequestEntity Entity { get; set; }
        IPod Pod { get; set; }
    }
}