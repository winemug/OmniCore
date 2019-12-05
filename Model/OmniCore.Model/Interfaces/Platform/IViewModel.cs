using System.ComponentModel;
using System.Threading.Tasks;

namespace OmniCore.Model.Interfaces.Platform
{
    public interface IViewModel : INotifyPropertyChanged
    {
        Task Initialize();
        Task Dispose();
    }
}
