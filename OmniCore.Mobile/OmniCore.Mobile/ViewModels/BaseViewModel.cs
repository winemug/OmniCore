using OmniCore.Model.Interfaces;
using OmniCore.Model.Interfaces.Data;
using OmniCore.Model.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;

namespace OmniCore.Mobile.ViewModels
{
    public class BaseViewModel : PropertyChangedImpl, IDisposable
    {
        private IPod pod;
        public IPod Pod { get => pod; set => SetProperty(ref pod, value) ; }

        public BaseViewModel()
        {
            App.Instance.PodProvider.ManagerChanged += PodProvider_PodChanged;
            AttachToCurrentPod();
        }

        private void PodProvider_PodChanged(object sender, EventArgs e)
        {
            AttachToCurrentPod();
        }

        private void AttachToCurrentPod()
        {
            if (Pod != null)
            {
                Pod.PropertyChanged -= Pod_PropertyChanged;
            }

            if (App.Instance.PodProvider.PodManager != null)
            {
                Pod = App.Instance.PodProvider.PodManager.Pod;
                Pod.PropertyChanged += Pod_PropertyChanged;
            }
            else
            {
                Pod = null;
            }
            OnPodPropertyChanged(this, new PropertyChangedEventArgs(string.Empty));
        }

        private void Pod_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            OnPodPropertyChanged(sender, e);
        }

        protected virtual void OnPodPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
        }

        bool isBusy = false;
        public bool IsBusy
        {
            get { return isBusy; }
            set { SetProperty(ref isBusy, value); }
        }

        string title = string.Empty;
        public string Title
        {
            get { return title; }
            set { SetProperty(ref title, value); }
        }

        private bool disposedValue = false; // To detect redundant calls
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    App.Instance.PodProvider.ManagerChanged -= PodProvider_PodChanged;
                    if (Pod != null)
                    {
                        Pod.PropertyChanged -= Pod_PropertyChanged;
                    }
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
