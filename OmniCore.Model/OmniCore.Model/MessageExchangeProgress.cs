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

        private string actionText;

        private IMessageExchangeStatistics statistics;
        private IMessageExchangeResult result;

        public bool CanBeCanceled { get => canBeCanceled; set => SetProperty(ref canBeCanceled, value); }

        public bool Waiting { get => waiting; set => SetProperty(ref waiting, value); }
        public bool Running { get => running; set => SetProperty(ref running, value); }
        public int Progress { get => progress; set => SetProperty(ref progress, value); }
        public bool Finished { get => finished; set => SetProperty(ref finished, value); }

        public IMessageExchangeStatistics Statistics { get => statistics; set => SetProperty(ref statistics, value); }
        public IMessageExchangeResult Result { get => result; set => SetProperty(ref result, value); }
        public IConversation Conversation { get; set; }

        public string ActionText { get => actionText; set => SetProperty(ref actionText, value); }

        public CancellationToken Token { get => Conversation.Token; }

        public event PropertyChangedEventHandler PropertyChanged;

        public MessageExchangeProgress(IConversation conversation, RequestType type, string parameters = null)
        {
            Conversation = conversation;
            Result = new MessageExchangeResult() { Source = Conversation.RequestSource, Type = type, Parameters = parameters };
        }

        public void SetException(Exception exception)
        {
            Result.Success = false;
            var oe = exception as OmniCoreException;
            Result.Failure = oe?.FailureType ?? FailureType.Unknown;
            Result.Exception = exception;
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
