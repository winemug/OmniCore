using System.ComponentModel;
using OmniCore.Model.Interfaces.Common;

namespace OmniCore.Model.Interfaces.Client
{
    public interface IViewModel : INotifyPropertyChanged, IDisposablesContainer, IClientResolvable
    {
        object Parameter { get; }
        void SetParameters(IView view, bool viaShell, object parameter);
    }
}