using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace OmniCore.Client.Interfaces
{
    public interface IViewModel : IDisposable, INotifyPropertyChanged
    {
        IList<IDisposable> Disposables { get; }
    }
}
