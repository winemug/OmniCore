using OmniCore.Model.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace OmniCore.Model
{
    public class MessageProgress : IMessageExchangeProgress
    {
        private bool canBeCanceled;
        private bool waiting;
        private bool running;
        private int progress;
        private bool finished;
        private bool successful;
        private int outgoingSuccess;
        private int outgoingFail;
        private int incomingSuccess;
        private int incomingFail;
        private IMessageExchangeStatistics statistics;

        public bool CanBeCanceled { get => canBeCanceled; set => SetProperty(ref canBeCanceled, value); }
        public bool Waiting { get => waiting; set => SetProperty(ref waiting, value); }
        public bool Running { get => running; set => SetProperty(ref running,  value); }
        public int Progress { get => progress; set => SetProperty(ref progress, value); }
        public bool Finished { get => finished; set => SetProperty(ref finished, value); }
        public bool Successful { get => successful; set => SetProperty(ref successful, value); }
        public int OutgoingSuccess { get => outgoingSuccess; set => SetProperty(ref outgoingSuccess, value); }
        public int OutgoingFail { get => outgoingFail; set => SetProperty(ref outgoingFail, value); }
        public int IncomingSuccess { get => incomingSuccess; set => SetProperty(ref incomingSuccess, value); }
        public int IncomingFail { get => incomingFail; set => SetProperty(ref incomingFail, value); }
        public IMessageExchangeStatistics Statistics { get => statistics; set => SetProperty(ref statistics, value); }

        public event PropertyChangedEventHandler PropertyChanged;

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
