using OmniCore.Mobile.Base;
using OmniCore.Model.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using OmniCore.Model.Enums;
using Xamarin.Forms;

namespace OmniCore.Mobile.ViewModels
{
    public abstract class BaseViewModel : PropertyChangedImpl, IDisposable
    {
        public IPod Pod { get; set; }

        public IConversation ActiveConversation { get; set; }

        public bool IsPodRunning { get; set; }

        public bool IsInConversation { get; set; }

        public bool CanRunCommand => IsPodRunning & !IsInConversation;

        protected List<IDisposable> Disposables = new List<IDisposable>();

        public BaseViewModel()
        {
            MessagingCenter.Subscribe<IPodProvider>(this, MessagingConstants.PodsChanged, (pp) =>
            {
                this.Pod = pp.SinglePod;
                var podState = this.Pod?.LastStatus?.Progress;
                IsPodRunning = podState != null && podState.Value >= PodProgress.Running &&
                             podState.Value <= PodProgress.RunningLow;
                IsInConversation = false;
                ActiveConversation = null;
                OnPropertyChanged(nameof(CanRunCommand));
            });

            MessagingCenter.Subscribe<IConversation>(this, MessagingConstants.ConversationStarted, (conversation)
                =>
            {
                IsInConversation = true;
                ActiveConversation = conversation;
                OnPropertyChanged(nameof(CanRunCommand));
            });

            MessagingCenter.Subscribe<IConversation>(this, MessagingConstants.ConversationEnded, (conversation)
                =>
            {
                IsInConversation = false;
                ActiveConversation = null;
                OnPropertyChanged(nameof(CanRunCommand));
            });
        }

        protected abstract void OnDisposeManagedResources();

        protected abstract Task<BaseViewModel> BindData();

        public async Task<BaseViewModel> DataBind()
        {
            Pod = App.Instance.PodProvider.SinglePod;
            await BindData();
            return this;
        }

        protected bool disposedValue = false; // To detect redundant calls
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    MessagingCenter.Unsubscribe<IPodProvider>(this, MessagingConstants.PodsChanged);
                    MessagingCenter.Unsubscribe<IConversation>(this, MessagingConstants.ConversationStarted);
                    MessagingCenter.Unsubscribe<IConversation>(this, MessagingConstants.ConversationEnded);
                    foreach(var disposable in Disposables)
                    {
                        disposable.Dispose();
                    }
                    Disposables.Clear();
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
