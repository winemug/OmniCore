using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Threading.Tasks;
using Nito.AsyncEx;
using Nito.AsyncEx.Synchronous;
using OmniCore.Model.Interfaces.Services;

namespace OmniCore.Client.ViewModels.Base
{
    //[Fody.ConfigureAwait(true)]
    public abstract class BaseViewModel : IViewModel
    {
        protected ICoreBootstrapper Bootstrapper { get; private set; }
        public BaseViewModel(ICoreBootstrapper bootstrapper)
        {
            Bootstrapper = bootstrapper;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public string Title { get; set; }
        public IView<IViewModel> View { get; set; }
        public abstract Task Initialize();
        public abstract Task Dispose();
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
