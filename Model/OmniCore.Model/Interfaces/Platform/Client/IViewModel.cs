using System;
using System.ComponentModel;
using System.Threading.Tasks;

namespace OmniCore.Model.Interfaces.Platform.Common
{
    public interface IViewModel : INotifyPropertyChanged, IDisposablesContainer, IClientResolvable
    {
        object Parameter { get; }
        void SetParameters(IView view, bool viaShell, object parameter);
    }
}
