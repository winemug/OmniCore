using System;
using System.ComponentModel;

namespace OmniCore.Model.Interfaces
{
    public interface IViewModel : INotifyPropertyChanged, IDisposable
    {
        void Initialize(IView view, bool viaShell, object parameter);
    }
}