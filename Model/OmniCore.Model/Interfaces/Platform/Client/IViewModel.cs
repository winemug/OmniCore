using System;
using System.ComponentModel;
using System.Threading.Tasks;

namespace OmniCore.Model.Interfaces.Common
{
    public interface IViewModel : INotifyPropertyChanged, IDisposableHandler, IClientResolvable
    {
        void InitializeModel(IView view);
    }

    public interface IViewModel<in T> : IViewModel
    {
        void InitializeModel(IView view, T parameter);
    }
}
