using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using OmniCore.Mobile.Annotations;
using Unity;
using Xamarin.Forms;

namespace OmniCore.Mobile.ViewModels
{
    public class BaseViewModel : INotifyPropertyChanged
    {
        protected Page Page;

        public bool IsShown { get; set; }
        protected bool IsModelInitialized { get; set; }
        
        protected static IUnityContainer UnityContainer => App.Container;
        public BaseViewModel()
        {
            IsModelInitialized = false;
        }
                
        protected virtual async Task PageShownAsync()
        {
        }

        protected virtual async Task PageHiddenAsync()
        {
        }
        
        public async Task OnNavigatedToAsync(Page page)
        {
            Page = page;
            if (!IsModelInitialized)
            {
                await InitializeAsync();
                IsModelInitialized = true;
                page.BindingContext = this;
            }
            if (!IsShown)
            {
                Debug.WriteLine($"*** Page is shown: {page.GetType()}");
                IsShown = true;
                await PageShownAsync();
            }
        }

        public async Task OnNavigatedFromAsync()
        {
            if (IsShown)
            {
                Debug.WriteLine($"*** Page is hidden: {Page.GetType()}");
                IsShown = false;
                await PageHiddenAsync();
            }
        }

        protected virtual Task InitializeAsync()
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