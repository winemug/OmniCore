using System.ComponentModel;
using System.Threading.Tasks;

namespace OmniCore.Model.Interfaces.Services
{
    public interface IViewModel : INotifyPropertyChanged
    {
        string Title { get; set; }
        IView<IViewModel> View { get; set; }
        Task Initialize();
        Task Dispose();
    }
}
