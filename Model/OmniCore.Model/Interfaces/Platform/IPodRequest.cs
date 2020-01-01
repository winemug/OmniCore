using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using OmniCore.Model.Interfaces.Data.Entities;
using OmniCore.Model.Interfaces.Platform;

namespace OmniCore.Model.Interfaces.Platform
{
    public interface IPodRequest : ITask, INotifyPropertyChanged
    {
        IPodRequestEntity Entity { get; set; }
        IPod Pod { get; set; }
    }
}