using OmniCore.Client.Constants;
using OmniCore.Model.Enums;
using OmniCore.Model.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace OmniCore.Client.ViewModels.Pod
{
    public class OverviewViewModel : PageViewModel
    {

        public string Id { get; set; }
        public string Updated { get; set; }
        public string Status { get; set; }
        public string LifetimeRemaining { get; set; }
        public string LifetimeActive { get; set; }
        public Color LifetimeColor { get; set; }
        public double LifetimeProgress { get; set; }
        public string ReservoirRemaining { get; set; }
        public string ReservoirDelivered { get; set; }
        public Color ReservoirColor { get; set; }
        public double ReservoirProgress { get; set; }
        public string BasalStatus { get; set; }
        public string BasalText1 { get; set; }
        public string BasalText2 { get; set; }

        public OverviewViewModel(Page page) : base(page)
        {
            //Disposables.Add(this.OnPropertyChanges().Subscribe((propertyName) =>
            //{
            //    if (propertyName == nameof(Pod))
            //    {
            //        Pod?.LastStatus?.UpdateWithEstimates(Pod);
            //    }
            //}));

            //MessagingCenter.Subscribe<IStatus>(this, MessagingConstants.PodStatusUpdated,
            //    (newStatus) => { Pod?.LastStatus?.UpdateWithEstimates(Pod); });
        }

        private bool TimerShouldRun = false;

        [method: SuppressMessage("", "CS1998", Justification = "Not applicable")]
        protected async override Task OnAppearing()
        {
            TimerShouldRun = true;

            Device.StartTimer(TimeSpan.FromSeconds(10), () =>
            {
                UpdateViewModelProperties();
                return TimerShouldRun;
            });
        }

        [method: SuppressMessage("", "CS1998", Justification = "Not applicable")]
        protected async override Task OnDisappearing()
        {
            TimerShouldRun = false;
        }

        [method: SuppressMessage("", "CS1998", Justification = "Not applicable")]
        protected async override Task<BaseViewModel> BindData()
        {
            UpdateViewModelProperties();
            return this;
        }

        protected override void OnDisposeManagedResources()
        {
            MessagingCenter.Unsubscribe<IStatus>(this, MessagingConstants.PodStatusUpdated);
        }

        private void UpdateViewModelProperties()
        {
            //if (Pod == null)
            //{
            //    Id = string.Empty;
            //    Status = "No pod";
            //}

            //if (Pod?.LastStatus == null)
            //{
            //    Updated = string.Empty;

            //    LifetimeRemaining = string.Empty;
            //    LifetimeActive = string.Empty;
            //    LifetimeColor = Color.Beige;
            //    LifetimeProgress = 0;

            //    ReservoirRemaining = string.Empty;
            //    ReservoirDelivered = string.Empty;
            //    ReservoirColor = Color.Beige;
            //    ReservoirProgress = 0;

            //    BasalStatus = string.Empty;
            //    BasalText1 = string.Empty;
            //    BasalText2 = string.Empty;
            //}
            //else
            //{
            //    Id = Pod.Lot.HasValue
            //        ? $"L{Pod.Lot} T{Pod.Serial} R0x{Pod.RadioAddress:X8}"
            //        : $"R0x{Pod.RadioAddress:X8}";
            //    Status = GetStatusText(Pod.LastStatus.Progress);
            //    Updated = GetTimeAgoText(DateTimeOffset.UtcNow - Pod.LastStatus.Created);

            //    if (Pod.LastStatus.ActiveMinutesEstimate.HasValue)
            //    {
            //        var ts = TimeSpan.FromMinutes(4800 - Pod.LastStatus.ActiveMinutesEstimate.Value);
            //        LifetimeRemaining = $"{ts.Days}d {ts.Hours}h {ts.Minutes}m";

            //        ts = TimeSpan.FromMinutes(Pod.LastStatus.ActiveMinutesEstimate.Value);
            //        LifetimeActive = $"{ts.Days}d {ts.Hours}h {ts.Minutes}m";

            //        if (Pod.LastStatus.ActiveMinutesEstimate < 24 * 60 * 3)
            //            LifetimeColor = Color.LightGreen;
            //        else if (Pod.LastStatus.ActiveMinutesEstimate < 24 * 60 * 3)
            //            LifetimeColor = Color.Green;
            //        else if (Pod.LastStatus.ActiveMinutesEstimate < 24 * 60 * 3)
            //            LifetimeColor = Color.GreenYellow;
            //        else
            //            LifetimeColor = Color.IndianRed;

            //        if (Pod.LastStatus.ActiveMinutes >= 4800)
            //            LifetimeProgress = 0;
            //        else
            //            LifetimeProgress = (4800 - Pod.LastStatus.ActiveMinutesEstimate.Value) / 4800.0;
            //    }
            //    else
            //    {
            //        LifetimeRemaining = string.Empty;
            //        LifetimeActive = string.Empty;
            //        LifetimeColor = Color.Beige;
            //        LifetimeProgress = 0;
            //    }

            //    if (Pod.LastStatus.Progress < PodProgress.RunningLow)
            //        ReservoirRemaining = "more than 50U";
            //    else
            //        ReservoirRemaining = Pod.LastStatus.ReservoirEstimate.HasValue
            //            ? $"{Pod.LastStatus.ReservoirEstimate.Value:F2}U"
            //            : "Less than 50U";

            //    if (Pod.LastStatus.DeliveredInsulinEstimate.HasValue)
            //    {
            //        if (Pod.ReservoirUsedForPriming.HasValue)
            //            ReservoirDelivered =
            //                $"{Pod.LastStatus.DeliveredInsulinEstimate.Value - Pod.ReservoirUsedForPriming.Value:F2}U";
            //        else
            //            ReservoirDelivered = $"{Pod.LastStatus.DeliveredInsulinEstimate.Value:F2}U";
            //    }
            //    else
            //    {
            //        ReservoirDelivered = $"{Pod.LastStatus.DeliveredInsulin:F2}U";
            //    }

            //    if (Pod.LastStatus.Progress < PodProgress.RunningLow)
            //        ReservoirColor = Color.LightGreen;

            //    if (Pod.LastStatus.ReservoirEstimate < 10)
            //        ReservoirColor = Color.Red;
            //    else
            //        ReservoirColor = Color.Yellow;

            //    if (Pod.LastStatus.ReservoirEstimate.Value >= 50m)
            //        ReservoirProgress = 1;
            //    else
            //        ReservoirProgress = (double) Pod.LastStatus.ReservoirEstimate.Value / 50.0;

            //    if (Pod.LastStatus.BasalStateEstimate == BasalState.Scheduled)
            //    {
            //        BasalStatus = "Basal Active";
            //    }
            //    else
            //    {
            //        if (Pod.LastStatus.BasalStateEstimate == BasalState.Suspended ||
            //            (Pod.LastStatus.TemporaryBasalRate.HasValue && Pod.LastStatus.TemporaryBasalRate.Value == 0m))
            //        {
            //            BasalStatus = "Basal Suspended";
            //        }
            //        else
            //        {
            //            BasalStatus = "Temporary Basal Active";
            //        }
            //    }

            //    BasalText1 = GetBasalText1();
            //    BasalText2 = GetBasalText2();
            //}
        }

        //private string GetStatusText(PodProgress? progress)
        //{
        //    switch (progress)
        //    {
        //        case PodProgress.InitialState:
        //        case PodProgress.TankPowerActivated:
        //        case PodProgress.TankFillCompleted:
        //            return $"Not yet paired";
        //        case PodProgress.PairingSuccess:
        //            return $"Paired";
        //        case PodProgress.Purging:
        //            return $"Priming";
        //        case PodProgress.ReadyForInjection:
        //            return $"Ready for Insertion";
        //        case PodProgress.BasalScheduleSet:
        //        case PodProgress.Priming:
        //            return $"Starting";
        //        case PodProgress.Running:
        //            return $"Running";
        //        case PodProgress.RunningLow:
        //            return $"Running (Low Reservoir)";
        //        case PodProgress.ErrorShuttingDown:
        //            return $"Error";
        //        case PodProgress.AlertExpiredShuttingDown:
        //            return $"Expired";
        //        case PodProgress.Inactive:
        //            return $"Deactivated";
        //        default:
        //            return "Unknown";
        //    }
        //}

        //private string GetTimeAgoText(TimeSpan ts)
        //{
        //    if (ts.TotalSeconds < 15)
        //        return $"Just now";
        //    else if (ts.TotalMinutes < 1)
        //        return $"Less than a minute ago";
        //    else if (ts.TotalMinutes < 2)
        //        return $"1 minute ago";
        //    else if (ts.TotalMinutes < 3)
        //        return $"2 minutes ago";
        //    else if (ts.TotalMinutes < 60)
        //        return $"{(int) ts.TotalMinutes} minutes ago";
        //    else if (ts.TotalMinutes < 120)
        //    {
        //        if (ts.TotalMinutes < 70)
        //            return $"1 hour ago";
        //        else
        //            return $"more than 1 hour ago";
        //    }
        //    else if (ts.TotalHours < 24)
        //        return $"{(int) ts.TotalHours} hours ago";
        //    else if (ts.TotalHours < 25)
        //        return $"1 day ago";
        //    else if (ts.TotalHours < 48)
        //        return $"More than 1 day ago";
        //    else
        //        return $"{(int) ts.TotalDays} days ago";
        //}
        //private string GetBasalText1()
        //{
        //    if (Pod == null)
        //        return string.Empty;
        //    else if (Pod.LastStatus != null && Pod.LastStatus.BasalStateEstimate.HasValue
        //                                    && Pod.LastStatus.BasalStateEstimate == BasalState.Scheduled)
        //    {
        //        if (Pod.LastStatus.ScheduledBasalRate.HasValue)
        //            return $"{Pod.LastStatus.ScheduledBasalRate:F2} U/h";
        //        else
        //            return $"Rate unknown";
        //    }
        //    else if (Pod.LastStatus != null && Pod.LastStatus.BasalStateEstimate.HasValue
        //                                    && Pod.LastStatus.BasalStateEstimate == BasalState.Temporary)
        //    {
        //        if (Pod.LastStatus.TemporaryBasalRate.HasValue)
        //        {
        //            if (Pod.LastStatus.TemporaryBasalTotalHours == 0.5m)
        //                return $"{Pod.LastStatus.TemporaryBasalRate:F2} U/h for half an hour";
        //            else if (Pod.LastStatus.TemporaryBasalTotalHours == 1m)
        //                return $"{Pod.LastStatus.TemporaryBasalRate:F2} U/h for 1 hour";
        //            else
        //                return $"{Pod.LastStatus.TemporaryBasalRate:F2} U/h for {Pod.LastStatus.TemporaryBasalTotalHours:F1} hours";
        //        }
        //        return $"Rate and duration unknown";
        //    }
        //    else
        //        return string.Empty;
        //}

        //private string GetBasalText2()
        //{
        //    if (Pod == null)
        //        return string.Empty;
        //    else if (Pod.LastStatus != null && Pod.LastStatus.BasalStateEstimate.HasValue
        //                                    && Pod.LastStatus.BasalStateEstimate == BasalState.Scheduled
        //                                    && Pod.LastStatus.ScheduledBasalAverage.HasValue)
        //    {
        //        return $"(Average basal rate {Pod.LastStatus.ScheduledBasalAverage:F2} U/h)";
        //    }
        //    else if (Pod.LastStatus != null && Pod.LastStatus.BasalStateEstimate.HasValue
        //                                    && Pod.LastStatus.BasalStateEstimate == BasalState.Temporary
        //                                    && Pod.LastStatus.TemporaryBasalRemaining.HasValue)
        //    {
        //        var remaining = Pod.LastStatus.TemporaryBasalRemaining.Value;

        //        if (remaining.TotalMinutes < 1)
        //            return $"(Less than a minute remaining)";
        //        else if (remaining.TotalMinutes < 2)
        //            return $"(1 minute remaining)";
        //        else if (remaining.TotalMinutes < 120)
        //            return $"({(int)remaining.TotalMinutes} minutes remaining)";
        //        else if (remaining.Minutes == 0)
        //            return $"({remaining.Hours} hours remaining)";
        //        else if (remaining.Minutes == 1)
        //            return $"({remaining.Hours} hours and 1 minute remaining)";
        //        else
        //            return $"({remaining.Hours} hours and {remaining.Minutes} minute remaining)";
        //    }
        //    else
        //        return string.Empty;
        //}
    }
}
