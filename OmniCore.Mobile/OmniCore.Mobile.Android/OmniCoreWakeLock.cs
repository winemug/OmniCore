using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using OmniCore.Mobile.Base.Interfaces;

namespace OmniCore.Mobile.Android
{
    public class OmniCoreWakeLock : IWakeLock
    {
        private PowerManager.WakeLock AndroidWakeLock;

        public OmniCoreWakeLock(PowerManager.WakeLock wakeLock)
        {
            AndroidWakeLock = wakeLock;
        }
        public bool IsAcquired => AndroidWakeLock.IsHeld;

        public async Task<bool> Acquire(int timeout)
        {
            await Task.Run(() => AndroidWakeLock.Acquire(timeout));
            return AndroidWakeLock.IsHeld;
        }

        public void Release()
        {
            if (AndroidWakeLock.IsHeld)
            {
                AndroidWakeLock.Release();
                AndroidWakeLock = null;
            }
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                    if (AndroidWakeLock != null)
                        Release();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~OmniCoreWakeLock()
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
        #endregion
    }
}