using Newtonsoft.Json;
using OmniCore.Model.Enums;
using OmniCore.Model.Interfaces;
using OmniCore.Model.Interfaces.Data;
using OmniCore.Model.Utilities;
using SQLite;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace OmniCore.Model.Eros.Data
{
    [Table("Status")]
    public class ErosStatus : PropertyChangedImpl, IStatus
    {
        [PrimaryKey, AutoIncrement]
        public long? Id { get; set; }
        public Guid PodId { get; set; }
        public DateTime Created { get; set; }

        public bool Faulted { get; set; }

        public decimal NotDeliveredInsulin { get; set; }

        public decimal DeliveredInsulin { get; set; }
        public decimal Reservoir { get; set; }

        public PodProgress Progress { get; set; }
        public BasalState BasalState { get; set; }
        public BolusState BolusState { get; set; }

        public uint ActiveMinutes { get; set; }

        public byte AlertMask { get; set; }

        private int _message_seq = 0;
        
        public int MessageSequence
        {
            get => _message_seq;
            set
            {
                _message_seq = value % 16;
            }
        }

        public decimal DeliveredInsulinEstimate { get; set; }
        public decimal ReservoirEstimate { get; set; }
        public uint ActiveMinutesEstimate { get; set; }
        public BasalState BasalStateEstimate { get; set; }
        public BolusState BolusStateEstimate { get; set; }

        public decimal? TemporaryBasalTotalHours { get; set; }
        public TimeSpan? TemporaryBasalRemaining { get; set; }
        public decimal? TemporaryBasalRate { get; set; }
        public decimal? ScheduledBasalRate { get; set; }
        public decimal? ScheduledBasalAverage { get; set; }

        public void UpdateWithEstimates(IPod pod)
        {
            DeliveredInsulinEstimate = DeliveredInsulin;
            ReservoirEstimate = Reservoir;
            ActiveMinutesEstimate = ActiveMinutes;
            BasalStateEstimate = BasalState;
            BolusStateEstimate = BolusState;

            var utcNow = DateTime.UtcNow;

            TimeSpan timePast = utcNow - Created;

            if (!Faulted && BolusState == BolusState.Immediate)
            {
                var shouldHaveDelivered = (decimal)(timePast.TotalSeconds / 2) * 0.05m;
                if (shouldHaveDelivered > NotDeliveredInsulin)
                    shouldHaveDelivered = NotDeliveredInsulin;

                shouldHaveDelivered -= shouldHaveDelivered % 0.05m;

                DeliveredInsulinEstimate += shouldHaveDelivered;

                if (ReservoirEstimate < 50.0m)
                    ReservoirEstimate -= shouldHaveDelivered;

                if (shouldHaveDelivered == NotDeliveredInsulin)
                {
                    BolusStateEstimate = BolusState.Inactive;
                }
            }

            var basalInsulinEstimate = 0m;

            if (BasalState == BasalState.Temporary)
            {
                if (pod.LastTempBasalResult != null)
                {
                    var anon = new { BasalRate = 0m, Duration = 0m };

                    var parameters = JsonConvert.DeserializeAnonymousType(pod.LastTempBasalResult.Parameters, anon);

                    var tempBasalEnd = pod.LastTempBasalResult.ResultTime.Value.AddHours((double)parameters.Duration);
                    if (tempBasalEnd > utcNow)
                    {
                        TemporaryBasalTotalHours = parameters.Duration;
                        TemporaryBasalRate = parameters.BasalRate;
                        TemporaryBasalRemaining = tempBasalEnd - utcNow;

                        basalInsulinEstimate += parameters.Duration * parameters.BasalRate;
                        BasalStateEstimate = BasalState.Scheduled;
                        basalInsulinEstimate += GetScheduledBasalTotals(tempBasalEnd, utcNow, pod);
                    }
                    else
                    {
                        basalInsulinEstimate += (decimal)timePast.TotalHours * parameters.BasalRate;
                    }
                }
            }
            else
            {
                basalInsulinEstimate += GetScheduledBasalTotals(Created, utcNow, pod);
            }

            basalInsulinEstimate -= basalInsulinEstimate % 0.05m;

            if (Reservoir < 50.0m)
            {
                ReservoirEstimate -= basalInsulinEstimate;
            }

            ActiveMinutesEstimate = ActiveMinutes + (uint)timePast.TotalMinutes;

            if (pod.LastBasalSchedule != null)
            {
                ScheduledBasalRate = pod.LastBasalSchedule.BasalSchedule[CurrentHalfHourIndex(utcNow)];
                ScheduledBasalAverage = pod.LastBasalSchedule.BasalSchedule.Average();
            }

        }

        private decimal GetScheduledBasalTotals(DateTime start, DateTime end, IPod pod)
        {
            if (pod.LastBasalSchedule == null)
                return 0m;

            var podTime1 = start + TimeSpan.FromMinutes(pod.LastBasalSchedule.UtcOffset);
            var podTime2 = end + TimeSpan.FromMinutes(pod.LastBasalSchedule.UtcOffset);

            decimal scheduledEstimate = 0;
            var podTimeCurrent = podTime1;
            while (true)
            {
                var currentRate = pod.LastBasalSchedule.BasalSchedule[CurrentHalfHourIndex(podTimeCurrent)];
                var podTimeNext = NextHalfHour(podTimeCurrent);
                if (podTimeNext > podTime2)
                {
                    scheduledEstimate += (currentRate / 2);
                }
                else
                {
                    var ratio = (decimal)((podTime2 - podTimeCurrent).TotalMinutes / 30.0);
                    if (ratio > 0)
                        scheduledEstimate += (currentRate / 2) * ratio;
                    break;
                }
            }
            return scheduledEstimate;
        }

        private int CurrentHalfHourIndex(DateTime current)
        {
            var hh = current.Hour * 2;
            if (current.Minute >= 30)
                hh++;
            return hh;
        }

        private DateTime NextHalfHour(DateTime current)
        {
            if (current.Minute < 30)
            {
                return new DateTime(current.Year, current.Month, current.Day, current.Hour, 30, 0, DateTimeKind.Utc);
            }
            else
            {
                current = current.AddMinutes(30);
                return new DateTime(current.Year, current.Month, current.Day, current.Hour, 0, 0, DateTimeKind.Utc);
            }
        }
    }
}
