using SQLite;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace OmniCore.Model.Eros
{
    public class ErosRepository
    {
        private static ErosRepository instance;
        public static ErosRepository Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new ErosRepository();
                }
                return instance;
            }
        }

        private string DbPath;
        private string DbConnectionString;

        private ErosRepository()
        {
            DbPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "omnicore.db3");
            DbConnectionString = $"Data Source={DbPath}";
            Initialize();
        }

        public void Initialize()
        {
            try
            {
                using (var conn = new SQLiteConnection(DbPath))
                {
                    conn.CreateTable<ErosPod>();
                    conn.CreateTable<ErosPodAlertStates>();
                    conn.CreateTable<ErosPodBasalSchedule>();
                    conn.CreateTable<ErosPodFault>();
                    conn.CreateTable<ErosPodRadioIndicators>();
                    conn.CreateTable<ErosPodStatus>();
                    conn.CreateTable<ErosPodUserSettings>();
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
                return conn.Table<ErosPod>()
                    .FirstOrDefault(x => !x.Archived);
            }
        }

        public ErosPod Load(uint lot, uint tid)
        {
            using (var conn = GetConnection())
            {
                return conn.Table<ErosPod>()
                    .FirstOrDefault(x => x.Lot == lot && x.Serial == tid);
            }
        }

        public ErosPod GetLastActivated()
        {
            using (var conn = GetConnection())
            {
                return conn.Table<ErosPod>().OrderByDescending(x => x.ActivationDate)
                    .FirstOrDefault();
            }
        }

        public void Save(ErosPod pod)
        {
            using (var conn = new SQLiteConnection(DbPath))
            {
                conn.InsertOrReplace(pod);
            }
        }
    }
}
