using OmniCore.Mobile.Base;
using OmniCore.Model.Interfaces;
using OmniCore.Model.Interfaces.Data;
using OmniCore.Model.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace OmniCore.Mobile.ViewModels
{
    [Fody.ConfigureAwait(true)]
    public abstract class BaseViewModel : PropertyChangedImpl, IDisposable
    {
        protected Page AssociatedPage;
        protected PropertyChangedDependencyHandler DependencyHandler;

        public IPod Pod { get; set; }

        [DependencyPath(nameof(Pod), nameof(IPod.ActiveConversation), nameof(IConversation.IsFinished))]
        public bool PodExistsAndNotBusy
        {
            get
            {
                return Pod != null
                    && (Pod.ActiveConversation == null || Pod.ActiveConversation.IsFinished);
            }
        }

        [DependencyPath(nameof(Pod), nameof(IPod.ActiveConversation), nameof(IConversation.IsFinished))]
        public bool PodNotBusy
        {
            get
            {
                return (Pod?.ActiveConversation == null || Pod.ActiveConversation.IsFinished);
            }
        }

        public BaseViewModel(Page page)
        {
            AssociatedPage = page;
            page.Appearing += Page_Appearing;
            page.Disappearing += Page_Disappearing;
            App.Instance.PodProvider.ManagerChanged += PodProvider_PodChanged;
        }

        //private bool IsViewModelInitialized = false;
        private async void Page_Appearing(object sender, EventArgs e)
        {
            DependencyHandler = new PropertyChangedDependencyHandler(this);
            Pod = App.Instance.PodProvider.PodManager?.Pod;
            var data = await BindData();
            await OnAppearing();
        }

        private async void Page_Disappearing(object sender, EventArgs e)
        {
            await OnDisappearing();
            App.Instance.PodProvider.ManagerChanged -= PodProvider_PodChanged;
            DependencyHandler?.Dispose();
        }

        protected abstract Task<object> BindData();

        protected abstract void OnDisposeManagedResources();

        protected async virtual Task OnAppearing()
        {
        }

        protected async virtual Task OnDisappearing()
        {
        }

        private void PodProvider_PodChanged(object sender, EventArgs e)
        {
            if (App.Instance.PodProvider.PodManager != null)
            {
                Pod = App.Instance.PodProvider.PodManager?.Pod;
            }
            else
            {
                Pod = null;
            }
            OnPropertyChanged(nameof(PodExistsAndNotBusy));
            OnPropertyChanged(nameof(PodNotBusy));
        }

        private bool disposedValue = false; // To detect redundant calls
        private void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    DependencyHandler?.Dispose();

                    if (AssociatedPage != null)
                    {
                        AssociatedPage.Appearing -= Page_Appearing;
                        AssociatedPage.Disappearing -= Page_Disappearing;
                    }

                    App.Instance.PodProvider.ManagerChanged -= PodProvider_PodChanged;
                    OnDisposeManagedResources();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~BaseViewModel()
        // {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
    }
}
