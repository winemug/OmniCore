using OmniCore.Model.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using OmniCore.Repository.Enums;
using Xamarin.Forms;

namespace OmniCore.Client.ViewModels
{
    public abstract class BaseViewModel : PropertyChangedImpl, IDisposable
    {
        protected List<IDisposable> Disposables = new List<IDisposable>();

        public BaseViewModel()
        {
        }

        protected abstract void OnDisposeManagedResources();

        protected abstract Task<BaseViewModel> BindData();

        public async Task<BaseViewModel> DataBind()
        {
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
