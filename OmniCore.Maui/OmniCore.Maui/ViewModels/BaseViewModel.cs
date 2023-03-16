using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace OmniCore.Maui.ViewModels
{
    public class BaseViewModel : INotifyPropertyChanged, IAsyncDisposable
    {
        public async ValueTask DisposeAsync()
        {
        }
        public event PropertyChangedEventHandler PropertyChanged;

        public async Task RaisePropertyChangedAsync(string propertyName = null)
        {
            await MainThread.InvokeOnMainThreadAsync(() => { OnPropertyChanged(propertyName); });
        }

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public virtual async ValueTask OnAppearing()
        {
        }

        public virtual async ValueTask OnDisappearing()
        {
        }
    }
}