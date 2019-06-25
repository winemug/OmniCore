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
        private const int MAX_RECORDS = 50;
        private ObservableCollection<ResultViewModel> results;

        public ObservableCollection<ResultViewModel> Results { get => results; set => SetProperty(ref results, value); }

        public ConversationsViewModel()
        {
            Results = new ObservableCollection<ResultViewModel>();
            var history = ErosRepository.Instance.GetHistoricalResultsForDisplay(MAX_RECORDS);
            foreach (var result in history)
                Results.Add(new ResultViewModel(result));
            OnPodChanged();
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
            OnPropertyChanged(nameof(ConversationTitle));
            OnPropertyChanged(nameof(ConversationIntent));
            OnPropertyChanged(nameof(Started));
            OnPropertyChanged(nameof(Ended));
            OnPropertyChanged(nameof(StartedBy));
            OnPropertyChanged(nameof(this.RequestPhase));
            OnPropertyChanged(nameof(this.ExchangeActionResult));
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

                OnPodChanged();
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
                OnPodChanged();
            }
        }

        private async void ExchangeProgress_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(IMessageExchangeProgress.Result))
                await UpdateRunningResult(exchangeProgress?.Result);
            OnPodChanged();
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

        public string ConversationTitle
        {
            get
            {
                if (conversation == null)
                    return "No active conversation";
                else if (conversation.IsFinished)
                    return "Last Conversation";
                else
                    return "Active Conversation";
            }
        }

        public string ConversationIntent
        {
            get
            {
                if (conversation == null)
                    return "";
                else return conversation.Intent;
            }
        }

        public string Started
        {
            get
            {
                if (conversation != null)
                    return conversation.Started.ToLocalTime().ToString("hh:mm:ss");
                return "";
            }
        }

        public string Ended
        {
            get
            {
                if (conversation != null && conversation.Ended.HasValue)
                    return conversation.Ended.Value.ToLocalTime().ToString("hh:mm:ss");
                return "";
            }
        }

        public string StartedBy
        {
            get
            {
                if (conversation == null)
                    return "";
                else
                    switch (conversation.RequestSource)
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

        public string RequestPhase
        {
            get
            {
                if (exchangeProgress == null)
                    return string.Empty;
                else
                {
                    if (exchangeProgress.Waiting)
                        return "Waiting to be run";
                    if (exchangeProgress.Finished)
                        return "Finished";
                    if (exchangeProgress.Running)
                    {
                        if (exchangeProgress.Result.RequestTime.HasValue)
                        {
                            var diff = DateTimeOffset.UtcNow - exchangeProgress.Result.RequestTime.Value;
                            if (diff.TotalSeconds < 4)
                                return $"Running";
                            else
                                return $"Running for {diff.TotalSeconds:F0} seconds";
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
                if (exchangeProgress == null)
                    return string.Empty;
                else if (exchangeProgress.Finished)
                {
                    if (exchangeProgress.Result.Success)
                        return "Result received";
                    else
                        return $"Messsage exchange failed: {exchangeProgress.Result.Failure}";
                }
                else
                {
                    return exchangeProgress.ActionText;
                }
            }
        }
    }
}
