using OmniCore.Mobile.Base;
using OmniCore.Model.Enums;
using OmniCore.Model.Eros;
using OmniCore.Model.Interfaces;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace OmniCore.Mobile.ViewModels.Pod
{
    public class ConversationsViewModel : PageViewModel
    {
        private const int MAX_RECORDS = 10;

        public ObservableCollection<ResultViewModel> Results { get; set; }

        public ConversationsViewModel(Page page):base(page)
        {
        }

        protected async override Task<BaseViewModel> BindData()
        {
            Results = new ObservableCollection<ResultViewModel>();
            var repo = await ErosRepository.GetInstance();
            var history = await repo.GetHistoricalResultsForDisplay(MAX_RECORDS).ConfigureAwait(true);
            foreach (var result in history)
            {
                Results.Add((ResultViewModel)await new ResultViewModel(result).DataBind());
            }

            MessagingCenter.Subscribe<IMessageExchangeResult>(this, MessagingConstants.NewResultReceived,
                async (newResult) =>
                {
                    await AddNewResult(newResult);
                });

            return this;
        }

        protected override void OnDisposeManagedResources()
        {
            MessagingCenter.Unsubscribe<IMessageExchangeResult>(this, MessagingConstants.NewResultReceived);
            foreach (var result in Results)
                result.Dispose();
        }

        private async Task AddNewResult(IMessageExchangeResult newResult)
        {
            await OmniCoreServices.Application.RunOnMainThread(() =>
            {
                if (Results.Count > 0)
                    Results.Insert(0, new ResultViewModel(newResult));
                else
                    Results.Add(new ResultViewModel(newResult));

                if (Results.Count > MAX_RECORDS)
                    Results.RemoveAt(Results.Count - 1);
            });
        }

        public string ConversationTitle
        {
            get
            {
                var b = ActiveConversation?.IsFinished;
                if (!b.HasValue)
                    return "No active conversation";
                else if (b.Value)
                    return "Last Conversation";
                else
                    return "Active Conversation";
            }
        }

        public string ConversationIntent
        {
            get
            {
                return ActiveConversation?.Intent;
            }
        }

        public string Started
        {
            get
            {
                return ActiveConversation?.Started.ToLocalTime().ToString("hh:mm:ss");
            }
        }

        public string Ended
        {
            get
            {
                return ActiveConversation?.Ended?.ToLocalTime().ToString("hh:mm:ss");
            }
        }

        public string StartedBy
        {
            get
            {
                switch (ActiveConversation?.RequestSource)
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
                var exchangeProgress = ActiveConversation?.CurrentExchange;
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
                        var t = exchangeProgress.Result.RequestTime;
                        if (t.HasValue)
                        {
                            var diff = DateTimeOffset.UtcNow - t.Value;
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
                var exchangeProgress = ActiveConversation?.CurrentExchange;
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
