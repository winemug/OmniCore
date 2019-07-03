using OmniCore.Mobile.Base;
using OmniCore.Mobile.Base.Interfaces;
using OmniCore.Model.Enums;
using OmniCore.Model.Eros.Data;
using OmniCore.Model.Exceptions;
using OmniCore.Model.Interfaces;
using OmniCore.Model.Interfaces.Data;
using OmniCore.Model.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OmniCore.Model.Eros
{
    public class ErosConversation : PropertyChangedImpl, IConversation
    {
        public DateTimeOffset Started { get; set; }
        public DateTimeOffset? Ended { get; set; }
        public string Intent { get; set; }
        public IMessageExchangeStatistics CombinedStatistics { get; set; }

        public bool CanCancel { get; set; }

        public bool IsRunning { get; set; }
        public bool IsFinished { get; set; }

        public bool Failed { get; set; }
        public bool Canceled { get; set; }

        public FailureType FailureType { get; set; }
        public RequestSource RequestSource { get; set; }

        public Exception Exception
        {
            get => exception;
            set
            {
                CanCancel = false;
                IsRunning = false;
                IsFinished = true;
                Failed = true;
                var oe = value as OmniCoreException;
                FailureType = oe?.FailureType ?? FailureType.Unknown;
                exception = value;
                OnPropertyChanged(nameof(this.Exception));
            }
        }

        public IMessageExchangeProgress CurrentExchange { get; set; }

        public CancellationToken Token => CancellationTokenSource.Token;

        private IPod Pod;
        private Exception exception;
        private readonly SemaphoreSlim ConversationMutex;
        private readonly IWakeLock WakeLock;
        private readonly CancellationTokenSource CancellationTokenSource;
        private TaskCompletionSource<bool> CancellationCompletion;

        public ErosConversation(SemaphoreSlim conversationMutex, IWakeLock wakeLock, IPod pod)
        {
            Started = DateTimeOffset.UtcNow;
            ConversationMutex = conversationMutex;
            CancellationTokenSource = new CancellationTokenSource();
            Pod = pod;
            CombinedStatistics = new ErosMessageExchangeStatistics();
        }

        public async Task<bool> Cancel()
        {
            if (!CanCancel)
                return false;

            if (!CancellationTokenSource.IsCancellationRequested)
            {
                CancellationCompletion = new TaskCompletionSource<bool>();
                CancellationTokenSource.Cancel();
            }

            var result = await CancellationCompletion.Task;
            if (result)
            {
                IsRunning = false;
                IsFinished = true;
                Canceled = true;
            }
            return result;
        }

        public void CancelComplete()
        {
            if (CancellationTokenSource.IsCancellationRequested)
            {
                CanCancel = false;
                CancellationCompletion.TrySetResult(true);
            }
        }

        public void CancelFailed()
        {
            if (CancellationTokenSource.IsCancellationRequested)
            {
                CanCancel = false;
                CancellationCompletion.TrySetResult(false);
            }
        }

        public IMessageExchangeProgress NewExchange(IMessage requestMessage)
        {
            if (CurrentExchange != null)
            {
                if (!CurrentExchange.Finished)
                {
                    throw new OmniCoreWorkflowException(FailureType.WorkflowError, "Cannot start a new exchange while one is already running");
                }
            }
            CurrentExchange = new MessageExchangeProgress(this, requestMessage.RequestType, requestMessage.Parameters);
            OnPropertyChanged(nameof(CurrentExchange));
            return CurrentExchange;
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    IsFinished = true;
                    Ended = DateTimeOffset.UtcNow;
                    ConversationMutex.Release();
                    WakeLock.Release();
                    CancellationTokenSource.Dispose();
                    //Pod.ActiveConversation = null;
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~ErosConversation()
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
