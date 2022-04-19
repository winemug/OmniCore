using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using OmniCore.Mobile.Annotations;
using OmniCore.Mobile.Services;
using Unity;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace OmniCore.Mobile.ViewModels
{
    public class BaseViewModel : INotifyPropertyChanged, IDisposable
    {
        protected Page Page;
        protected static IUnityContainer UnityContainer => App.Container;
        
        protected NavigationService NavigationService { get; }

        public BaseViewModel()
        {
            NavigationService = UnityContainer.Resolve<NavigationService>();
        }
        
        public async Task BindToPageAsync(Page page)
        {
            Page = page;
            await OnBeforeBindAsync();
            page.BindingContext = this;
        }
        
        public async Task NavigatedToAsync()
        {
            await OnPageShownAsync();
        }

        public async Task NavigatedAwayAsync()
        {
            await OnPageHiddenAsync();
        }

        public async Task RaisePropertyChangedAsync(string propertyName = null)
        {
            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                OnPropertyChanged(propertyName);
            });
        }
        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void Dispose()
        {
            OnDispose();
        }
        
        protected virtual async Task OnPageShownAsync()
        {
        }

        protected virtual async Task OnPageHiddenAsync()
        {
        }
        
        protected virtual async Task OnBeforeBindAsync()
        {
        }
        
        protected virtual void OnDispose()
        {
        }

    }
}