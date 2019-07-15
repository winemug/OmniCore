using OmniCore.Mobile.Base;
using OmniCore.Model.Interfaces;
using OmniCore.Model.Interfaces.Data;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace OmniCore.Mobile.ViewModels
{
    public abstract class BaseViewModel : PropertyChangedImpl, IDisposable
    {
        public IPod Pod { get; set; }

        public bool PodExistsAndNotBusy
        {
            get
            {
                return Pod != null
                    && (Pod.ActiveConversation == null || Pod.ActiveConversation.IsFinished);
            }
        }

        public bool PodNotBusy
        {
            get
            {
                return (Pod?.ActiveConversation == null || Pod.ActiveConversation.IsFinished);
            }
        }

        public BaseViewModel()
        {
            MessagingCenter.Subscribe<IPodProvider>(this, MessagingConstants.PodChanged, (pp) =>
            {
                this.Pod = pp.PodManager?.Pod;
            });
        }

        protected abstract void OnDisposeManagedResources();

        protected abstract Task<BaseViewModel> BindData();

        public async Task<BaseViewModel> DataBind()
        {
            Pod = App.Instance.PodProvider.PodManager?.Pod;
            await BindData();
            return this;
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
        }

        protected bool disposedValue = false; // To detect redundant calls
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    MessagingCenter.Unsubscribe<IPodProvider>(this, MessagingConstants.PodChanged);
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
