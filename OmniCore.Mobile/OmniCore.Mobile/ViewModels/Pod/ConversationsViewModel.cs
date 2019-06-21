using OmniCore.Mobile.Base;
using OmniCore.Model.Enums;
using OmniCore.Model.Eros;
using OmniCore.Model.Interfaces;
using OmniCore.Model.Interfaces.Data;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;

namespace OmniCore.Mobile.ViewModels.Pod
{
    public class ConversationsViewModel : BaseViewModel
    {
        private const int MAX_RECORDS = 10;
        private ObservableCollection<ResultViewModel> results;

        public ObservableCollection<ResultViewModel> Results { get => results; set => SetProperty(ref results, value); }

        public ConversationsViewModel()
        {
            Results = new ObservableCollection<ResultViewModel>();
            var history = ErosRepository.Instance.GetHistoricalResultsForDisplay(MAX_RECORDS);
            foreach (var result in history)
                Results.Add(new ResultViewModel(result));
        }

        protected override void OnDisposeManagedResources()
        {
            if (exchangeProgress != null)
                exchangeProgress.PropertyChanged -= ExchangeProgress_PropertyChanged;
            if (conversation == null)
                conversation.PropertyChanged -= Conversation_PropertyChanged;

            foreach (var result in Results)
                result.Dispose();
        }

        protected override async void OnPodChanged()
        {
            await UpdateRunningResult(exchangeProgress?.Result);
        }

        private IConversation conversation;
        protected override async void OnPodPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(IPod.ActiveConversation))
            {
                if (conversation != null)
                    conversation.PropertyChanged -= Conversation_PropertyChanged;

                conversation = Pod.ActiveConversation;

                if (conversation != null)
                {
                    conversation.PropertyChanged += Conversation_PropertyChanged;
                    await UpdateRunningResult(conversation.CurrentExchange?.Result);
                }
            }
        }

        private IMessageExchangeProgress exchangeProgress;
        private async void Conversation_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(IConversation.CurrentExchange))
            {
                if (exchangeProgress != null)
                    exchangeProgress.PropertyChanged -= ExchangeProgress_PropertyChanged;
                exchangeProgress = conversation.CurrentExchange;
                if (exchangeProgress != null)
                {
                    exchangeProgress.PropertyChanged += ExchangeProgress_PropertyChanged;
                    await UpdateRunningResult(exchangeProgress?.Result);
                }
            }
        }

        private async void ExchangeProgress_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(IMessageExchangeProgress.Result))
                await UpdateRunningResult(exchangeProgress?.Result);
        }

        private IMessageExchangeResult activeResult = null;
        private async Task UpdateRunningResult(IMessageExchangeResult newResult)
        {
            if (newResult != null)
            {
                await OmniCoreServices.Application.RunOnMainThread(() =>
                {
                    lock (this)
                    {
                        if (activeResult != newResult)
                        {
                            activeResult = newResult;
                            if (results.Count > 0)
                                results.Insert(0, new ResultViewModel(newResult));
                            else
                                results.Add(new ResultViewModel(newResult));

                            if (results.Count > MAX_RECORDS)
                                results.RemoveAt(results.Count - 1);
                        }
                    }
                });
            }
        }

        //public string ConversationTitle
        //{
        //    get
        //    {
        //        if (CurrentConversation == null)
        //            return "No active conversation";
        //        else if (CurrentConversation.IsFinished)
        //            return "Last Conversation";
        //        else
        //            return "Active Conversation";
        //    }
        //}

        //public string ConversationIntent
        //{
        //    get
        //    {
        //        if (CurrentConversation == null)
        //            return "";
        //        else return CurrentConversation.Intent;
        //    }
        //}

        //public string Started
        //{
        //    get
        //    {
        //        if (CurrentConversation != null)
        //            return CurrentConversation.Started.ToLocalTime().ToString("hh:MM:ss");
        //        return "";
        //    }
        //}

        //public string Ended
        //{
        //    get
        //    {
        //        if (CurrentConversation != null && CurrentConversation.Ended.HasValue)
        //            return CurrentConversation.Ended.Value.ToLocalTime().ToString("hh:MM:ss");
        //        return "";
        //    }
        //}

        //public string StartedBy
        //{
        //    get
        //    {
        //        if (CurrentConversation == null)
        //            return "";
        //        else
        //            switch(CurrentConversation.RequestSource)
        //            {
        //                case RequestSource.AndroidAPS:
        //                    return "Android APS";
        //                case RequestSource.OmniCoreUser:
        //                    return "OmniCore User";
        //                case RequestSource.OmniCoreRemoteUser:
        //                case RequestSource.OmniCoreAID:
        //                default:
        //                    return "";
        //            }
        //    }
        //}

        //public string RequestType
        //{
        //    get
        //    {
        //        return CurrentExchange?.Result?.Type.ToString();
        //    }
        //}

        //public string RequestPhase
        //{
        //    get
        //    {
        //        if (CurrentExchange == null)
        //            return string.Empty;
        //        else
        //        {
        //            if (CurrentExchange.Waiting)
        //                return "Waiting to be run";
        //            if (CurrentExchange.Finished)
        //                return "Finished";
        //            if (CurrentExchange.Running)
        //            {
        //                if (CurrentExchange.Result.RequestTime.HasValue)
        //                {
        //                    var diff = DateTimeOffset.UtcNow - CurrentExchange.Result.RequestTime.Value;
        //                    if (diff.TotalSeconds < 4)
        //                        return $"Running";
        //                    else
        //                        return $"Running for {diff.TotalSeconds} seconds";
        //                }
        //                else
        //                    return $"Running";
        //            }
        //            return "Unknown";
        //        }
        //    }
        //}

        //public string ExchangeActionResult
        //{
        //    get
        //    {
        //        if (CurrentExchange == null)
        //            return string.Empty;
        //        else if (CurrentExchange.Finished)
        //        {
        //            if (CurrentExchange.Result.Success)
        //                return "Result received";
        //            else
        //                return $"Messsage exchange failed: {CurrentExchange.Result.Failure}";
        //        }
        //        else
        //        {
        //            return currentExchange.ActionText;
        //        }
        //    }
        //}

        //private IConversation conversation;
        //public IConversation CurrentConversation
        //{
        //    get => conversation;
        //    set
        //    {
        //        if (value != null && conversation != value)
        //        {
        //            if (conversation != null)
        //                conversation.PropertyChanged -= Conversation_PropertyChanged;
        //            conversation = value;
        //            if (conversation != null)
        //                conversation.PropertyChanged += Conversation_PropertyChanged;

        //            CurrentExchange = conversation?.CurrentExchange;
        //            OnPropertyChanged("");
        //        }
        //    }
        //}

        //private IMessageExchangeProgress currentExchange;
        //public IMessageExchangeProgress CurrentExchange
        //{
        //    get => currentExchange;
        //    set
        //    {
        //        if (currentExchange != value)
        //        {
        //            if (currentExchange != null)
        //            {
        //                currentExchange.PropertyChanged -= Exchange_PropertyChanged;
        //                currentExchange.Result.PropertyChanged -= Result_PropertyChanged;
        //            }
        //            currentExchange = value;
        //            if (currentExchange != null)
        //            {
        //                currentExchange.PropertyChanged += Exchange_PropertyChanged;
        //                currentExchange.Result.PropertyChanged += Result_PropertyChanged;
        //            }

        //            OnPropertyChanged(nameof(CurrentExchange));
        //            OnPropertyChanged("");
        //        }
        //    }
        //}

        //public ConversationsViewModel()
        //{
        //    OnPodChanged();
        //}

        //protected override void OnPodChanged()
        //{
        //    CurrentConversation = Pod?.ActiveConversation;
        //    CurrentExchange = Pod?.ActiveConversation?.CurrentExchange;
        //    OnPropertyChanged(nameof(ConversationTitle));
        //    OnPropertyChanged(nameof(ConversationIntent));
        //    OnPropertyChanged(nameof(Started));
        //    OnPropertyChanged(nameof(Ended));
        //    OnPropertyChanged(nameof(StartedBy));
        //    OnPropertyChanged(nameof(this.RequestType));
        //    OnPropertyChanged(nameof(this.RequestPhase));
        //    OnPropertyChanged(nameof(this.ExchangeActionResult));

        //}

        //protected override void OnPodPropertyChanged(object sender, PropertyChangedEventArgs args)
        //{
        //    if (args.PropertyName == string.Empty || args.PropertyName == nameof(IPod.ActiveConversation))
        //        OnPodChanged();
        //}


        //private void Conversation_PropertyChanged(object sender, PropertyChangedEventArgs args)
        //{
        //    if (args.PropertyName == string.Empty || args.PropertyName == nameof(IConversation.CurrentExchange))
        //    {
        //        CurrentExchange = CurrentConversation?.CurrentExchange;
        //        OnPodChanged();
        //    }

        //    if (args.PropertyName == nameof(IConversation.RequestSource))
        //        OnPropertyChanged(nameof(StartedBy));

        //    if (args.PropertyName == nameof(IConversation.Started))
        //    {
        //        OnPropertyChanged(nameof(Started));
        //        OnPropertyChanged(nameof(ConversationTitle));
        //    }

        //    if (args.PropertyName == nameof(IConversation.Ended))
        //    {
        //        OnPropertyChanged(nameof(Ended));
        //        OnPropertyChanged(nameof(ConversationTitle));
        //    }

        //    if (args.PropertyName == nameof(IConversation.Intent))
        //        OnPropertyChanged(nameof(ConversationIntent));
        //}

        //private void Exchange_PropertyChanged(object sender, PropertyChangedEventArgs args)
        //{
        //    switch (args.PropertyName)
        //    {
        //        case "":
        //            OnPodChanged();
        //            break;
        //        case nameof(IMessageExchangeProgress.Waiting):
        //            OnPropertyChanged(nameof(this.RequestPhase));
        //            break;
        //        case nameof(IMessageExchangeProgress.Running):
        //            OnPropertyChanged(nameof(this.RequestPhase));
        //            break;
        //        case nameof(IMessageExchangeProgress.Finished):
        //            OnPropertyChanged(this.RequestPhase);
        //            OnPropertyChanged(nameof(this.ExchangeActionResult));
        //            break;
        //        case nameof(IMessageExchangeProgress.ActionText):
        //            OnPropertyChanged(nameof(this.ExchangeActionResult));
        //            break;
        //        default:
        //            break;
        //    }
        //}

        //private void Result_PropertyChanged(object sender, PropertyChangedEventArgs args)
        //{
        //    switch (args.PropertyName)
        //    {
        //        case "":
        //            OnPodChanged();
        //            break;
        //        case nameof(IMessageExchangeResult.Type):
        //            OnPropertyChanged(nameof(this.RequestType));
        //            break;
        //        case nameof(IMessageExchangeResult.RequestTime):
        //            OnPropertyChanged(nameof(this.RequestPhase));
        //            break;
        //        case nameof(IMessageExchangeResult.Success):
        //            OnPropertyChanged(nameof(this.ExchangeActionResult));
        //            break;
        //        case nameof(IMessageExchangeResult.Failure):
        //            OnPropertyChanged(nameof(this.ExchangeActionResult));
        //            break;
        //        default:
        //            break;
        //    }
        //}


        //protected override void OnDisposeManagedResources()
        //{
        //    CurrentExchange = null;
        //    CurrentConversation = null;
        //}
    }
}
