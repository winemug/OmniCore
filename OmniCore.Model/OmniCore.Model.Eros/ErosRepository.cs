using SQLite;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

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
            DbPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "omnicore.db3");
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
                    conn.CreateTable<ErosPodRadioIndicators>();
                    conn.CreateTable<ErosPodStatus>();
                    conn.CreateTable<ErosPodUserSettings>();
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

        public void Save(ErosPod pod)
        {
            using (var conn = new SQLiteConnection(DbPath))
            {
                conn.BeginTransaction();
                conn.InsertOrReplace(pod);
                if (pod.AlertStates != null)
                {
                    pod.AlertStates.PodId = pod.Id.Value;
                    conn.InsertOrReplace(pod.AlertStates);
                }
                if (pod.BasalSchedule != null)
                {
                    pod.BasalSchedule.PodId = pod.Id.Value;
                    conn.InsertOrReplace(pod.BasalSchedule);
                }
                if (pod.Fault != null)
                {
                    pod.Fault.PodId = pod.Id.Value;
                    conn.InsertOrReplace(pod.Fault);
                }
                if (pod.RadioIndicators!= null)
                {
                    pod.RadioIndicators.PodId = pod.Id.Value;
                    conn.InsertOrReplace(pod.RadioIndicators);
                }
                if (pod.Status != null)
                {
                    pod.Status.PodId = pod.Id.Value;
                    conn.InsertOrReplace(pod.Status);
                }
                if (pod.UserSettings != null)
                {
                    pod.AlertStates.PodId = pod.Id.Value;
                    conn.InsertOrReplace(pod.UserSettings);
                }
                conn.Commit();
            }
        }

        private ErosPod WithRelations(ErosPod pod, SQLiteConnection conn)
        {
            if (pod == null)
                return null;

            pod.AlertStates = conn.Table<ErosPodAlertStates>().Where(x => x.PodId == pod.Id).OrderByDescending(x => x.Id)
                .FirstOrDefault();

            pod.BasalSchedule = conn.Table<ErosPodBasalSchedule>().Where(x => x.PodId == pod.Id).OrderByDescending(x => x.Id)
                .FirstOrDefault();

            pod.Fault = conn.Table<ErosPodFault>().Where(x => x.PodId == pod.Id).OrderByDescending(x => x.Id)
                .FirstOrDefault();

            pod.RadioIndicators = conn.Table<ErosPodRadioIndicators>().Where(x => x.PodId == pod.Id).OrderByDescending(x => x.Id)
                .FirstOrDefault();

            pod.Status = conn.Table<ErosPodStatus>().Where(x => x.PodId == pod.Id).OrderByDescending(x => x.Id)
                .FirstOrDefault();

            pod.UserSettings = conn.Table<ErosPodUserSettings>().Where(x => x.PodId == pod.Id).OrderByDescending(x => x.Id)
                .FirstOrDefault();

            return pod;
        }
    }
}
