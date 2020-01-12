using System;
using System.ComponentModel;
using System.Threading.Tasks;

namespace OmniCore.Model.Interfaces.Common
{
    public interface IViewModel : INotifyPropertyChanged, IDisposableHandler, IClientResolvable
    {
        object Parameter { get; }
        void SetParameters(IView view, bool viaShell, object parameter);
    }
}
