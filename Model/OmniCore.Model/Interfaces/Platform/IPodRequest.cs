using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using OmniCore.Model.Interfaces.Data.Entities;
using OmniCore.Model.Interfaces.Services;

namespace OmniCore.Model.Interfaces.Services
{
    public interface IPodRequest : ITask, INotifyPropertyChanged
    {
        IPodRequestEntity Entity { get; set; }
        IPod Pod { get; set; }
    }
}