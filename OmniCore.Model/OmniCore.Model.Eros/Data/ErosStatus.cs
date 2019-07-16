using Newtonsoft.Json;
using OmniCore.Model.Enums;
using OmniCore.Model.Interfaces;
using OmniCore.Model.Utilities;
using SQLite;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using OmniCore.Mobile.Base;

namespace OmniCore.Model.Eros.Data
{
    [Table("Status")]
    public class ErosStatus : IStatus
    {
        [PrimaryKey, AutoIncrement]
        public long? Id { get; set; }
        public Guid PodId { get; set; }
        public DateTimeOffset Created { get; set; }

        public bool? Faulted { get; set; }

        public decimal? NotDeliveredInsulin { get; set; }

        public decimal? DeliveredInsulin { get; set; }
        public decimal? Reservoir { get; set; }

        public PodProgress? Progress { get; set; }
        public BasalState? BasalState { get; set; }
        public BolusState? BolusState { get; set; }

        public uint? ActiveMinutes { get; set; }

        public byte? AlertMask { get; set; }

        public decimal? DeliveredInsulinEstimate { get; set; }
        public decimal? ReservoirEstimate { get; set; }
        public uint? ActiveMinutesEstimate { get; set; }
        public BasalState? BasalStateEstimate { get; set; }
        public BolusState? BolusStateEstimate { get; set; }

        public decimal? TemporaryBasalTotalHours { get; set; }
        public TimeSpan? TemporaryBasalRemaining { get; set; }
        public decimal? TemporaryBasalRate { get; set; }
        public decimal? ScheduledBasalRate { get; set; }
        public decimal? ScheduledBasalAverage { get; set; }

        public void UpdateWithEstimates(IPod pod)
        {
            lock (this)
            {
                DeliveredInsulinEstimate = DeliveredInsulin;
                ReservoirEstimate = Reservoir;
                ActiveMinutesEstimate = ActiveMinutes;
                BasalStateEstimate = BasalState;
                BolusStateEstimate = BolusState;

                var utcNow = DateTimeOffset.UtcNow;

                TimeSpan timePast = utcNow - Created;
                if (timePast.TotalHours > 80)
                {
                    timePast = TimeSpan.FromHours(80);
                    utcNow = Created + timePast;
                }

                if (Faulted.HasValue && !Faulted.Value
                    && BolusState.HasValue && BolusState.Value == Enums.BolusState.Immediate
                    && NotDeliveredInsulin.HasValue)
                {
                    var shouldHaveDelivered = (decimal)(timePast.TotalSeconds / 2) * 0.05m;
                    if (shouldHaveDelivered > NotDeliveredInsulin.Value)
                        shouldHaveDelivered = NotDeliveredInsulin.Value;

                    shouldHaveDelivered -= shouldHaveDelivered % 0.05m;

                    if (DeliveredInsulinEstimate.HasValue)
                        DeliveredInsulinEstimate += shouldHaveDelivered;

                    if (ReservoirEstimate < 50.0m)
                        ReservoirEstimate -= shouldHaveDelivered;

                    if (shouldHaveDelivered == NotDeliveredInsulin)
                    {
                        BolusStateEstimate = Enums.BolusState.Inactive;
                    }
                }

                var basalInsulinEstimate = 0m;

                if (BasalState.HasValue && BasalState.Value == Enums.BasalState.Temporary)
                {
                    if (pod.LastTempBasalResult != null)
                    {
                        var anon = new { BasalRate = 0m, Duration = 0m };

                        var parameters = JsonConvert.DeserializeAnonymousType(pod.LastTempBasalResult.Parameters, anon);

                        var tempBasalEnd = pod.LastTempBasalResult.ResultTime.Value.AddHours((double)parameters.Duration);
                        if (tempBasalEnd < utcNow)
                        {
                            basalInsulinEstimate += parameters.Duration * parameters.BasalRate;
                            basalInsulinEstimate += GetScheduledBasalTotals(tempBasalEnd, utcNow, pod);
                            BasalStateEstimate = Enums.BasalState.Scheduled;
                        }
                        else
                        {
                            TemporaryBasalTotalHours = parameters.Duration;
                            TemporaryBasalRate = parameters.BasalRate;
                            TemporaryBasalRemaining = tempBasalEnd - utcNow;
                            basalInsulinEstimate += (decimal)timePast.TotalHours * parameters.BasalRate;
                            BasalStateEstimate = Enums.BasalState.Temporary;
                        }
                    }
                }
                else
                {
                    basalInsulinEstimate += GetScheduledBasalTotals(Created, utcNow, pod);
                }

                basalInsulinEstimate -= basalInsulinEstimate % 0.05m;

                if (Reservoir.HasValue && Reservoir < 50.0m)
                {
                    ReservoirEstimate -= basalInsulinEstimate;
                }
                if (DeliveredInsulinEstimate.HasValue)
                    DeliveredInsulinEstimate += basalInsulinEstimate;

                if (ActiveMinutesEstimate.HasValue)
                    ActiveMinutesEstimate = ActiveMinutes + (uint)timePast.TotalMinutes;

                if (pod.LastBasalSchedule != null)
                {
                    ScheduledBasalRate = pod.LastBasalSchedule.BasalSchedule[CurrentHalfHourIndex(utcNow)];
                    ScheduledBasalAverage = pod.LastBasalSchedule.BasalSchedule.Average();
                }
            }
        }

        private decimal GetScheduledBasalTotals(DateTimeOffset start, DateTimeOffset end, IPod pod)
        {
            if (pod.LastBasalSchedule == null)
                return 0m;

            var podTimeStart = start + TimeSpan.FromMinutes(pod.LastBasalSchedule.UtcOffset);
            var podTimeEnd = end + TimeSpan.FromMinutes(pod.LastBasalSchedule.UtcOffset);

            decimal scheduledEstimate = 0;
            var podTimeCurrent = podTimeStart;
            while (true)
            {
                var currentRate = pod.LastBasalSchedule.BasalSchedule[CurrentHalfHourIndex(podTimeCurrent)];
                var podTimeNext = NextHalfHour(podTimeCurrent);
                if (podTimeNext < podTimeEnd)
                {
                    var ratio = (decimal)((podTimeNext - podTimeCurrent).TotalMinutes / 30.0);
                    if (ratio > 0)
                        scheduledEstimate += (currentRate / 2) * ratio;
                    podTimeCurrent = podTimeNext;
                }
                else
                {
                    var ratio = (decimal)((podTimeEnd - podTimeCurrent).TotalMinutes / 30.0);
                    if (ratio > 0)
                        scheduledEstimate += (currentRate / 2) * ratio;
                    break;
                }
            }
            return scheduledEstimate;
        }

        private int CurrentHalfHourIndex(DateTimeOffset current)
        {
            var hh = current.Hour * 2;
            if (current.Minute >= 30)
                hh++;
            return hh;
        }

        private DateTimeOffset NextHalfHour(DateTimeOffset current)
        {
            if (current.Minute < 30)
            {
                return new DateTimeOffset(current.Year, current.Month, current.Day, current.Hour, 30, 0, current.Offset);
            }
            else
            {
                current = current.AddMinutes(30);
                return new DateTimeOffset(current.Year, current.Month, current.Day, current.Hour, 0, 0, current.Offset);
            }
        }
    }
}
