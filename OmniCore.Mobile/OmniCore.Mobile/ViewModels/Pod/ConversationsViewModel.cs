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
using Xamarin.Forms;

namespace OmniCore.Mobile.ViewModels.Pod
{
    public class ConversationsViewModel : BaseViewModel
    {
        private const int MAX_RECORDS = 10;

        public ObservableCollection<ResultViewModel> Results { get; set; }

        public ConversationsViewModel(Page page):base(page)
        {
        }

        protected override async Task<object> BindData()
        {
            Results = new ObservableCollection<ResultViewModel>();
            var history = await ErosRepository.Instance.GetHistoricalResultsForDisplay(MAX_RECORDS).ConfigureAwait(true);
            foreach (var result in history)
                Results.Add(new ResultViewModel(base.AssociatedPage, result));
            return this;
        }

        protected override void OnDisposeManagedResources()
        {
            foreach (var result in Results)
                result.Dispose();
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
                            if (Results.Count > 0)
                                Results.Insert(0, new ResultViewModel(base.AssociatedPage, newResult));
                            else
                                Results.Add(new ResultViewModel(base.AssociatedPage, newResult));

                            if (Results.Count > MAX_RECORDS)
                                Results.RemoveAt(Results.Count - 1);
                        }
                    }
                });
            }
        }

        [DependencyPath(nameof(Pod), nameof(IPod.ActiveConversation), nameof(IConversation.IsFinished))]
        public string ConversationTitle
        {
            get
            {
                if (Pod?.ActiveConversation == null)
                    return "No active conversation";
                else if (Pod.ActiveConversation.IsFinished)
                    return "Last Conversation";
                else
                    return "Active Conversation";
            }
        }

        [DependencyPath(nameof(Pod), nameof(IPod.ActiveConversation), nameof(IConversation.Intent))]
        public string ConversationIntent
        {
            get
            {
                if (Pod?.ActiveConversation == null)
                    return "";
                else return Pod.ActiveConversation.Intent;
            }
        }

        [DependencyPath(nameof(Pod), nameof(IPod.ActiveConversation), nameof(IConversation.Started))]
        public string Started
        {
            get
            {
                if (Pod?.ActiveConversation != null)
                    return Pod.ActiveConversation.Started.ToLocalTime().ToString("hh:mm:ss");
                return "";
            }
        }

        [DependencyPath(nameof(Pod), nameof(IPod.ActiveConversation), nameof(IConversation.Ended))]
        public string Ended
        {
            get
            {
                if (Pod?.ActiveConversation?.Ended != null)
                    return Pod.ActiveConversation.Ended.Value.ToLocalTime().ToString("hh:mm:ss");
                return "";
            }
        }

        [DependencyPath(nameof(Pod), nameof(IPod.ActiveConversation), nameof(IConversation.RequestSource))]
        public string StartedBy
        {
            get
            {
                if (Pod?.ActiveConversation == null)
                    return "";
                else
                    switch (Pod.ActiveConversation.RequestSource)
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

        [DependencyPath(nameof(Pod), nameof(IPod.ActiveConversation), nameof(IConversation.CurrentExchange), nameof(IMessageExchangeProgress.Waiting))]
        [DependencyPath(nameof(Pod), nameof(IPod.ActiveConversation), nameof(IConversation.CurrentExchange), nameof(IMessageExchangeProgress.Finished))]
        [DependencyPath(nameof(Pod), nameof(IPod.ActiveConversation), nameof(IConversation.CurrentExchange), nameof(IMessageExchangeProgress.Running))]
        [DependencyPath(nameof(Pod), nameof(IPod.ActiveConversation), nameof(IConversation.CurrentExchange), nameof(IMessageExchangeProgress.Result), nameof(IMessageExchangeResult.RequestTime))]
        public string RequestPhase
        {
            get
            {
                var exchangeProgress = Pod?.ActiveConversation?.CurrentExchange;
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

        [DependencyPath(nameof(Pod), nameof(IPod.ActiveConversation), nameof(IConversation.CurrentExchange), nameof(IMessageExchangeProgress.Finished))]
        [DependencyPath(nameof(Pod), nameof(IPod.ActiveConversation), nameof(IConversation.CurrentExchange), nameof(IMessageExchangeProgress.Result), nameof(IMessageExchangeResult.Success))]
        [DependencyPath(nameof(Pod), nameof(IPod.ActiveConversation), nameof(IConversation.CurrentExchange), nameof(IMessageExchangeProgress.Result), nameof(IMessageExchangeResult.Failure))]
        public string ExchangeActionResult
        {
            get
            {
                var exchangeProgress = Pod?.ActiveConversation?.CurrentExchange;
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
