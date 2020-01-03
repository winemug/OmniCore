using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using OmniCore.Model.Interfaces.Data.Entities;
using OmniCore.Model.Interfaces;

namespace OmniCore.Model.Interfaces
{
    public interface IPodRequest : ITask, INotifyPropertyChanged, IServerResolvable
    {
        IPodRequestEntity Entity { get; set; }
        IPod Pod { get; set; }
    }
}