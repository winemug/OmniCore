using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using OmniCore.Mobile.Annotations;
using Xamarin.Forms;

namespace OmniCore.Mobile.ViewModels
{
    public class BaseViewModel : INotifyPropertyChanged
    {
        protected Page Page;
        private bool IsModelInitialized;
        public BaseViewModel(Page page)
        {
            Page = page;
            Page.Appearing += OnAppearing;
            Page.Disappearing += OnDisappearing;
        }
        
        private async void OnAppearing(object sender, EventArgs e)
        {
            if (!IsModelInitialized)
            {
                await InitializeAsync();
                Page.BindingContext = this;
            }
            await PageAppearingAsync();
        }

        private async void OnDisappearing(object sender, EventArgs e)
        {
            await PageDisappearingAsync();
        }

        protected virtual Task InitializeAsync()
        {
            return Task.CompletedTask;
        }
        protected virtual Task PageAppearingAsync()
        {
            return Task.CompletedTask;
        }

        protected virtual Task PageDisappearingAsync()
        {
            return Task.CompletedTask;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}