using OmniCore.Model.Enums;
using OmniCore.Model.Interfaces;
using OmniCore.Model.Interfaces.Data;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Text;

namespace OmniCore.Mobile.ViewModels.Pod
{
    public class ConversationViewModel : BaseViewModel
    {
        public string ConversationTitle
        {
            get
            {
                if (CurrentConversation == null)
                    return "No active conversation";
                else if (CurrentConversation.IsFinished)
                    return "Last Conversation";
                else
                    return "Active Conversation";
            }
        }

        public string ConversationIntent
        {
            get
            {
                if (CurrentConversation == null)
                    return "";
                else return CurrentConversation.Intent;
            }
        }

        public string Started
        {
            get
            {
                if (CurrentConversation != null)
                    return CurrentConversation.Started.ToLocalTime().ToString("hh:MM:ss");
                return "";
            }
        }

        public string Ended
        {
            get
            {
                if (CurrentConversation != null && CurrentConversation.Ended.HasValue)
                    return CurrentConversation.Ended.Value.ToLocalTime().ToString("hh:MM:ss");
                return "";
            }
        }

        public string StartedBy
        {
            get
            {
                if (CurrentConversation == null)
                    return "";
                else
                    switch(CurrentConversation.RequestSource)
                    {
                        case RequestSource.AndroidAPS:
                            return "Android APS";
                        case RequestSource.OmniCoreUser:
                            return "OmniCore User";
                        case RequestSource.OmniCoreRemoteUser:
                        case RequestSource.OmniCoreAID:
                        default:
                            return "";
                    }
            }
        }

        public string RequestType
        {
            get
            {
                return CurrentExchange?.Result?.Type.ToString();
            }
        }

        public string RequestPhase
        {
            get
            {
                if (CurrentExchange == null)
                    return string.Empty;
                else
                {
                    if (CurrentExchange.Waiting)
                        return "Waiting to be run";
                    if (CurrentExchange.Finished)
                        return "Finished";
                    if (CurrentExchange.Running)
                    {
                        if (CurrentExchange.Result.RequestTime.HasValue)
                        {
                            var diff = DateTimeOffset.UtcNow - CurrentExchange.Result.RequestTime.Value;
                            if (diff.TotalSeconds < 4)
                                return $"Running";
                            else
                                return $"Running for {diff.TotalSeconds} seconds";
                        }
                        else
                            return $"Running";
                    }
                    return "Unknown";
                }
            }
        }

        public string ExchangeActionResult
        {
            get
            {
                if (CurrentExchange == null)
                    return string.Empty;
                else if (CurrentExchange.Finished)
                {
                    if (CurrentExchange.Result.Success)
                        return "Result received";
                    else
                        return $"Messsage exchange failed: {CurrentExchange.Result.Failure}";
                }
                else
                {
                    return currentExchange.ActionText;
                }
            }
        }

        private IConversation conversation;
        public IConversation CurrentConversation
        {
            get => conversation;
            set
            {
                if (value != null && conversation != value)
                {
                    if (conversation != null)
                        conversation.PropertyChanged -= Conversation_PropertyChanged;
                    conversation = value;
                    if (conversation != null)
                        conversation.PropertyChanged += Conversation_PropertyChanged;

                    CurrentExchange = conversation?.CurrentExchange;
                    OnPropertyChanged("");
                }
            }
        }

        private IMessageExchangeProgress currentExchange;
        public IMessageExchangeProgress CurrentExchange
        {
            get => currentExchange;
            set
            {
                if (currentExchange != value)
                {
                    if (currentExchange != null)
                    {
                        currentExchange.PropertyChanged -= Exchange_PropertyChanged;
                        currentExchange.Result.PropertyChanged -= Result_PropertyChanged;
                    }
                    currentExchange = value;
                    if (currentExchange != null)
                    {
                        currentExchange.PropertyChanged += Exchange_PropertyChanged;
                        currentExchange.Result.PropertyChanged += Result_PropertyChanged;
                    }

                    OnPropertyChanged(nameof(CurrentExchange));
                    OnPropertyChanged("");
                }
            }
        }

        public ConversationViewModel()
        {
            OnPodChanged();
        }

        protected override void OnPodChanged()
        {
            CurrentConversation = Pod?.ActiveConversation;
            CurrentExchange = Pod?.ActiveConversation?.CurrentExchange;
        }

        protected override void OnPodPropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            if (args.PropertyName == string.Empty || args.PropertyName == nameof(IPod.ActiveConversation))
                OnPodChanged();
        }


        private void Conversation_PropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            if (args.PropertyName == string.Empty || args.PropertyName == nameof(IConversation.CurrentExchange))
            {
                CurrentExchange = CurrentConversation?.CurrentExchange;
                OnPropertyChanged("");
            }

            if (args.PropertyName == nameof(IConversation.RequestSource))
                OnPropertyChanged(nameof(StartedBy));

            if (args.PropertyName == nameof(IConversation.Started))
            {
                OnPropertyChanged(nameof(Started));
                OnPropertyChanged(nameof(ConversationTitle));
            }

            if (args.PropertyName == nameof(IConversation.Ended))
            {
                OnPropertyChanged(nameof(Ended));
                OnPropertyChanged(nameof(ConversationTitle));
            }

            if (args.PropertyName == nameof(IConversation.Intent))
                OnPropertyChanged(nameof(ConversationIntent));
        }

        private void Exchange_PropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            switch (args.PropertyName)
            {
                case "":
                    OnPropertyChanged(string.Empty);
                    break;
                case nameof(IMessageExchangeProgress.Waiting):
                    OnPropertyChanged(nameof(this.RequestPhase));
                    break;
                case nameof(IMessageExchangeProgress.Running):
                    OnPropertyChanged(nameof(this.RequestPhase));
                    break;
                case nameof(IMessageExchangeProgress.Finished):
                    OnPropertyChanged(this.RequestPhase);
                    OnPropertyChanged(nameof(this.ExchangeActionResult));
                    break;
                case nameof(IMessageExchangeProgress.ActionText):
                    OnPropertyChanged(nameof(this.ExchangeActionResult));
                    break;
                default:
                    break;
            }
        }

        private void Result_PropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            if (args.PropertyName == string.Empty)
                OnPropertyChanged(string.Empty);

            switch (args.PropertyName)
            {
                case "":
                    OnPropertyChanged(string.Empty);
                    break;
                case nameof(IMessageExchangeResult.Type):
                    OnPropertyChanged(nameof(this.RequestType));
                    break;
                case nameof(IMessageExchangeResult.RequestTime):
                    OnPropertyChanged(nameof(this.RequestPhase));
                    break;
                case nameof(IMessageExchangeResult.Success):
                    OnPropertyChanged(nameof(this.ExchangeActionResult));
                    break;
                case nameof(IMessageExchangeResult.Failure):
                    OnPropertyChanged(nameof(this.ExchangeActionResult));
                    break;
                default:
                    break;
            }
        }


        protected override void OnDisposeManagedResources()
        {
            CurrentExchange = null;
            CurrentConversation = null;
        }
    }
}
