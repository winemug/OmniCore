using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Threading.Tasks;
using OmniCore.Client.Interfaces;
using Nito.AsyncEx;
using Nito.AsyncEx.Synchronous;

namespace OmniCore.Client.ViewModels.Base
{
    [Fody.ConfigureAwait(true)]
    public abstract class BaseViewModel : IViewModel
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public abstract Task Initialize();
        public abstract Task Dispose();
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
