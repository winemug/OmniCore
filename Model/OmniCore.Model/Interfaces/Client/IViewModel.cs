using System;
using System.ComponentModel;
using OmniCore.Model.Interfaces.Common;

namespace OmniCore.Model.Interfaces.Client
{
    public interface IViewModel : INotifyPropertyChanged, IDisposable, IClientInstance
    {
        void Initialize(IView view, bool viaShell, object parameter);
    }
}