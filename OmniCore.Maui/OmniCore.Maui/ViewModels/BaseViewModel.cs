using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace OmniCore.Maui.ViewModels
{
    public class BaseViewModel : INotifyPropertyChanged, IAsyncDisposable
    {
        protected Page Page;

        public async ValueTask DisposeAsync()
        {
            Debug.WriteLine($"Dispose async {GetType()}");
            if (Page != null)
            {
                // Page.LayoutChanged -= PageOnLayoutChanged;
                Page.Disappearing -= PageOnDisappearing;
                Page.Appearing -= PageOnAppearing;
            }

            Page = null;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public async Task SetPage(Page page)
        {
            Page = page;
            await OnBinding();
            page.BindingContext = this;
            page.Appearing += PageOnAppearing;
            page.Disappearing += PageOnDisappearing;
            // page.LayoutChanged += PageOnLayoutChanged;
        }

        public async Task RaisePropertyChangedAsync(string propertyName = null)
        {
            await MainThread.InvokeOnMainThreadAsync(() => { OnPropertyChanged(propertyName); });
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        // private async void PageOnLayoutChanged(object sender, EventArgs e)
        // {
        //     Debug.WriteLine($"Page OnLayoutChanged {Page.GetType()}");
        // }

        private async void PageOnDisappearing(object sender, EventArgs e)

        {
            Debug.WriteLine($"Page OnDisappearing {Page.GetType()}");
            await OnDisappearing();
        }

        private async void PageOnAppearing(object sender, EventArgs e)
        {
            Debug.WriteLine($"Page Appearing {Page.GetType()}");
            await OnAppearing();
        }

        protected virtual async ValueTask OnBinding()
        {
        }

        protected virtual async ValueTask OnAppearing()
        {
        }

        protected virtual async ValueTask OnDisappearing()
        {
        }
    }
}