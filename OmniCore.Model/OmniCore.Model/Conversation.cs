using OmniCore.Model.Enums;
using OmniCore.Model.Exceptions;
using OmniCore.Model.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OmniCore.Model
{
    public class Conversation : IConversation
    {
        private bool canCancel = true;

        private bool waiting;
        private bool running;
        private int progress;
        private bool finished;

        private string commandText;
        private string actionText;
        private string actionStatusText;
        private CancellationTokenSource CancellationTokenSource;
        private TaskCompletionSource<bool> CancelCompletion;

        public bool CanCancel { get => canCancel; set => SetProperty(ref canCancel, value); }
        public bool IsWaiting { get => waiting; set => SetProperty(ref waiting, value); }
        public bool IsRunning { get => running; set => SetProperty(ref running, value); }
        public bool IsFinished { get => finished; set => SetProperty(ref finished, value); }
        public int Progress { get => progress; set => SetProperty(ref progress, value); }



        public string CommandText { get => commandText; set => SetProperty(ref commandText, value); }
        public string ActionText { get => actionText; set => SetProperty(ref actionText, value); }
        public string ActionStatusText { get => actionStatusText; set => SetProperty(ref actionStatusText, value); }

        public CancellationToken Token { get => CancellationTokenSource.Token; }

        public event PropertyChangedEventHandler PropertyChanged;

        public Conversation()
        {
            CancellationTokenSource = new CancellationTokenSource();
            Result = new MessageExchangeResult();
        }

        public void SetException(Exception exception)
        {
            Result.Success = false;
            var oe = exception as OmniCoreException;
            Result.FailureType = oe?.FailureType ?? FailureType.Unknown;
            Result.Exception = exception;
        }

        public async Task<bool> Cancel()
        {
            if (!CanCancel)
                return false;

            if (!CancellationTokenSource.IsCancellationRequested)
            {
                CancelCompletion = new TaskCompletionSource<bool>();
                CancellationTokenSource.Cancel();
            }

            return await CancelCompletion.Task;
        }

        public void CancelComplete()
        {
            if (CancellationTokenSource.IsCancellationRequested)
            {
                CanCancel = false;
                CancelCompletion.TrySetResult(true);
            }
        }

        public void CancelFailed()
        {
            if (CancellationTokenSource.IsCancellationRequested)
            {
                CanCancel = false;
                CancelCompletion.TrySetResult(false);
            }
        }

        public IMessageExchangeProgress NewExchange()
        {
            return new MessageExchangeProgress();
        }

        protected bool SetProperty<T>(ref T backingStore, T value,
            [CallerMemberName]string propertyName = "",
            Action onChanged = null)
        {
            if (EqualityComparer<T>.Default.Equals(backingStore, value))
                return false;

            backingStore = value;
            onChanged?.Invoke();
            OnPropertyChanged(propertyName);
            return true;
        }

        protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            var changed = PropertyChanged;
            if (changed == null)
                return;

            changed.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    if (this.CancellationTokenSource != null)
                    {
                        CancellationTokenSource.Dispose();
                        CancellationTokenSource = null;
                    }
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~MessageProgress()
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
