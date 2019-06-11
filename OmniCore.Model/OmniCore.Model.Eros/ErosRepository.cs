using OmniCore.Model.Interfaces;
using SQLite;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using SQLiteNetExtensions.Extensions;

namespace OmniCore.Model.Eros
{
    public class ErosRepository
    {
        private static readonly ErosRepository instance = new ErosRepository();
        public static ErosRepository Instance
        {
            get
            {
                return instance;
            }
        }

        private readonly string DbPath;
        //private string DbConnectionString;

        private ErosRepository()
        {
            DbPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "omnicore.db3");
            //DbConnectionString = $"Data Source={DbPath}";
            Initialize();
        }

        public void Initialize()
        {
            try
            {
                using (var conn = new SQLiteConnection(DbPath))
                {
                    conn.BeginTransaction();
                    conn.CreateTable<ErosPod>();
                    conn.CreateTable<ErosPodAlertStates>();
                    conn.CreateTable<ErosPodBasalSchedule>();
                    conn.CreateTable<ErosPodFault>();
                    conn.CreateTable<ErosPodStatus>();
                    conn.CreateTable<ErosPodUserSettings>();
                    conn.CreateTable<MessageExchangeResult>();
                    conn.CreateTable<MessageExchangeStatistics>();
                    conn.Commit();
                }
            }
            catch (SQLiteException sle)
            {
                Console.WriteLine($"Error: {sle}");
                throw sle;
            }
        }

        public SQLiteConnection GetConnection()
        {
            return new SQLiteConnection(DbPath);
        }

        public ErosPod LoadCurrent()
        {
            using (var conn = GetConnection())
            {
                return WithRelations(conn.Table<ErosPod>()
                    .FirstOrDefault(x => !x.Archived), conn);
            }
        }

        public ErosPod Load(uint lot, uint tid)
        {
            using (var conn = GetConnection())
            {
                return WithRelations(conn.Table<ErosPod>()
                    .FirstOrDefault(x => x.Lot == lot && x.Serial == tid), conn);
            }
        }

        public ErosPod GetLastActivated()
        {
            using (var conn = GetConnection())
            {
                return WithRelations(conn.Table<ErosPod>().OrderByDescending(x => x.ActivationDate)
                    .FirstOrDefault(), conn);
            }
        }

        public void Save(IPod pod, IMessageExchangeResult result = null)
        {
            using (var conn = new SQLiteConnection(DbPath))
            {
                conn.BeginTransaction();
                conn.InsertOrReplace(pod);

                if (result != null)
                {
                    result.PodId = pod.Id.Value;
                    result.Id = conn.InsertOrReplace(result, typeof(MessageExchangeResult));

                    if (result.Statistics != null)
                    {
                        result.Statistics.PodId = pod.Id.Value;
                        result.Statistics.Id = conn.InsertOrReplace(result.Statistics, typeof(MessageExchangeStatistics));
                    }

                    if (result.AlertStates != null)
                    {
                        result.AlertStates.PodId = pod.Id.Value;
                        result.AlertStates.Created = DateTime.UtcNow;
                        result.AlertStates.Id = conn.InsertOrReplace(result.AlertStates, typeof(ErosPodAlertStates));
                        pod.LastAlertStates = result.AlertStates;
                    }

                    if (result.BasalSchedule != null)
                    {
                        result.BasalSchedule.PodId = pod.Id.Value;
                        result.BasalSchedule.Created = DateTime.UtcNow;
                        result.BasalSchedule.Id = conn.InsertOrReplace(result.BasalSchedule, typeof(ErosPodBasalSchedule));
                        pod.LastBasalSchedule = result.BasalSchedule;
                    }

                    if (result.Fault != null)
                    {
                        result.Fault.PodId = pod.Id.Value;
                        result.Fault.Created = DateTime.UtcNow;
                        result.Fault.Id = conn.InsertOrReplace(result.Fault, typeof(ErosPodFault));
                        pod.LastFault = result.Fault;
                    }

                    if (result.Status != null)
                    {
                        result.Status.PodId = pod.Id.Value;
                        result.Status.Created = DateTime.UtcNow;
                        result.Status.Id = conn.InsertOrReplace(result.Status, typeof(ErosPodStatus));
                        pod.LastStatus = result.Status;
                    }

                    if (result.UserSettings != null)
                    {
                        result.UserSettings.PodId = pod.Id.Value;
                        result.UserSettings.Created = DateTime.UtcNow;
                        result.UserSettings.Id = conn.InsertOrReplace(result.UserSettings, typeof(ErosPodUserSettings));
                        pod.LastUserSettings = result.UserSettings;
                    }
                }

                conn.Commit();
            }
        }

        private ErosPod WithRelations(ErosPod pod, SQLiteConnection conn)
        {
            if (pod == null)
                return null;

            pod.LastAlertStates = conn.Table<ErosPodAlertStates>().Where(x => x.PodId == pod.Id).OrderByDescending(x => x.Id)
                .FirstOrDefault();

            pod.LastBasalSchedule = conn.Table<ErosPodBasalSchedule>().Where(x => x.PodId == pod.Id).OrderByDescending(x => x.Id)
                .FirstOrDefault();

            pod.LastFault = conn.Table<ErosPodFault>().Where(x => x.PodId == pod.Id).OrderByDescending(x => x.Id)
                .FirstOrDefault();

            pod.LastStatus = conn.Table<ErosPodStatus>().Where(x => x.PodId == pod.Id).OrderByDescending(x => x.Id)
                .FirstOrDefault();

            pod.LastUserSettings = conn.Table<ErosPodUserSettings>().Where(x => x.PodId == pod.Id).OrderByDescending(x => x.Id)
                .FirstOrDefault();

            return pod;
        }
    }
}
