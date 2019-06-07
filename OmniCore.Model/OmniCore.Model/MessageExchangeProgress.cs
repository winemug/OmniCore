using OmniCore.Model.Enums;
using OmniCore.Model.Exceptions;
using OmniCore.Model.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace OmniCore.Model
{
    public class MessageExchangeProgress : IMessageExchangeProgress
    {
        private bool canBeCanceled = true;

        private bool waiting;
        private bool running;
        private int progress;
        private bool finished;

        private int outgoingSuccess;
        private int outgoingFail;
        private int incomingSuccess;
        private int incomingFail;

        private string commandText;
        private string actionText;
        private string actionStatusText;
        private IConversation ParentConversation;

        private IMessageExchangeStatistics statistics;
        private IMessageExchangeResult result;

        public bool CanBeCanceled { get => canBeCanceled; set => SetProperty(ref canBeCanceled, value); }

        public bool Waiting { get => waiting; set => SetProperty(ref waiting, value); }
        public bool Running { get => running; set => SetProperty(ref running, value); }
        public int Progress { get => progress; set => SetProperty(ref progress, value); }
        public bool Finished { get => finished; set => SetProperty(ref finished, value); }

        public int OutgoingSuccess { get => outgoingSuccess; set => SetProperty(ref outgoingSuccess, value); }
        public int OutgoingFail { get => outgoingFail; set => SetProperty(ref outgoingFail, value); }
        public int IncomingSuccess { get => incomingSuccess; set => SetProperty(ref incomingSuccess, value); }
        public int IncomingFail { get => incomingFail; set => SetProperty(ref incomingFail, value); }

        public IMessageExchangeStatistics Statistics { get => statistics; set => SetProperty(ref statistics, value); }
        public IMessageExchangeResult Result { get => result; set => SetProperty(ref result, value); }

        public string CommandText { get => commandText; set => SetProperty(ref commandText, value); }
        public string ActionText { get => actionText; set => SetProperty(ref actionText, value); }
        public string ActionStatusText { get => actionStatusText; set => SetProperty(ref actionStatusText, value); }

        public CancellationToken Token { get => ParentConversation.Token; }

        public event PropertyChangedEventHandler PropertyChanged;

        public MessageExchangeProgress(IConversation parentConversation)
        {
            ParentConversation = parentConversation;
            Result = new MessageExchangeResult();
        }

        public void SetException(Exception exception)
        {
            Result.Success = false;
            var oe = exception as OmniCoreException;
            Result.FailureType = oe?.FailureType ?? FailureType.Unknown;
            Result.Exception = exception;
        }

        public async Task<bool> CancelExchange()
        {
            if (!CanBeCanceled)
                return false;

            if (!Token.IsCancellationRequested)
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
                CanBeCanceled = false;
                CancelCompletion.TrySetResult(true);
            }
        }

        public void CancelFailed()
        {
            if (CancellationTokenSource.IsCancellationRequested)
            {
                CanBeCanceled = false;
                CancelCompletion.TrySetResult(false);
            }
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
    }
}
