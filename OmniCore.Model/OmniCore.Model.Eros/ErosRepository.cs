using OmniCore.Model.Interfaces;
using SQLite;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OmniCore.Model.Interfaces.Data;
using OmniCore.Model.Eros.Data;
using OmniCore.Model.Enums;
using OmniCore.Mobile.Base;
using OmniCore.Model.Data;
using Xamarin.Forms;
using Microsoft.AppCenter.Crashes;

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

        public readonly string DbPath;
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
                    conn.CreateTable<ErosAlertStates>();
                    conn.CreateTable<ErosBasalSchedule>();
                    conn.CreateTable<ErosFault>();
                    conn.CreateTable<ErosStatus>();
                    conn.CreateTable<ErosUserSettings>();
                    conn.CreateTable<ErosMessageExchangeParameters>();
                    conn.CreateTable<ErosMessageExchangeResult>();
                    conn.CreateTable<ErosMessageExchangeStatistics>();
                    conn.CreateTable<ErosProfile>();
                    conn.CreateTable<ErosRadioPreferences>();
                    conn.CreateTable<OmniCoreSettings>();

                    if (conn.Table<ErosProfile>().Count() == 0)
                    {
                        conn.Insert(new ErosProfile()
                        {
                            Created = DateTimeOffset.UtcNow,
                            BasalSchedule = new decimal[]
                                { 0.05m, 0.05m, 0.05m, 0.05m, 0.05m, 0.05m, 0.05m, 0.05m,
                                  0.05m, 0.05m, 0.05m, 0.05m, 0.05m, 0.05m, 0.05m, 0.05m,
                                  0.05m, 0.05m, 0.05m, 0.05m, 0.05m, 0.05m, 0.05m, 0.05m,
                                  0.05m, 0.05m, 0.05m, 0.05m, 0.05m, 0.05m, 0.05m, 0.05m,
                                  0.05m, 0.05m, 0.05m, 0.05m, 0.05m, 0.05m, 0.05m, 0.05m,
                                  0.05m, 0.05m, 0.05m, 0.05m, 0.05m, 0.05m, 0.05m, 0.05m
                                },
                            UtcOffset = 0
                        });
                    }

                    if (conn.Table<ErosRadioPreferences>().Count() == 0)
                    {
                        conn.Insert(new ErosRadioPreferences()
                        {
                            ConnectToAny = true
                        });
                    }

                    if (conn.Table<OmniCoreSettings>().Count() == 0)
                    {
                        conn.Insert(new OmniCoreSettings()
                        {
                            AcceptCommandsFromAAPS = true
                        });
                    }
                    conn.Commit();
                }
            }
            catch (SQLiteException sle)
            {
                Crashes.TrackError(sle);
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
                var t = conn.Table<ErosPod>().ToList();

                return WithRelations(conn.Table<ErosPod>()
                    .Where(x => !x.Archived)
                    .ToList()
                    .OrderByDescending(x => x.Created.Ticks)
                    .FirstOrDefault(), conn);
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

        public ErosRadioPreferences GetRadioPreferences()
        {
            using (var conn = GetConnection())
            {
                return conn.Table<ErosRadioPreferences>().FirstOrDefault();
            }
        }

        public OmniCoreSettings GetOmniCoreSettings()
        {
            using (var conn = GetConnection())
            {
                return conn.Table<OmniCoreSettings>().FirstOrDefault();
            }
        }

        public void SaveOmniCoreSettings(OmniCoreSettings settings)
        {
            using (var conn = GetConnection())
            {
                conn.Update(settings);
            }
        }

        public ErosPod GetLastActivated()
        {
            using (var conn = GetConnection())
            {
                return WithRelations(conn.Table<ErosPod>().OrderByDescending(x => x.ActivationDate.Value.Ticks)
                    .FirstOrDefault(), conn);
            }
        }

        public IProfile GetProfile()
        {
            using (var conn = new SQLiteConnection(DbPath))
            {
                return conn.Table<ErosProfile>()
                    .OrderByDescending(x => x.Id)
                    .FirstOrDefault();
            }
        }

        public void Save(IProfile profile)
        {
            using (var conn = new SQLiteConnection(DbPath))
            {
                conn.InsertOrReplace(profile, typeof(ErosProfile));
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
                    if (result.Statistics != null)
                    {
                        result.Statistics.PodId = pod.Id;
                        result.Statistics.Created = DateTimeOffset.UtcNow;
                        result.Statistics.BeforeSave();
                        conn.InsertOrReplace(result.Statistics, typeof(ErosMessageExchangeStatistics));
                        result.StatisticsId = result.Statistics.Id;
                    }

                    if (result.ExchangeParameters != null)
                    {
                        result.ExchangeParameters.PodId = pod.Id;
                        result.ExchangeParameters.Created = DateTimeOffset.UtcNow;
                        conn.InsertOrReplace(result.ExchangeParameters, typeof(ErosMessageExchangeParameters));
                        result.ParametersId = result.ExchangeParameters.Id;
                    }

                    if (result.Success && result.AlertStates != null)
                    {
                        result.AlertStates.PodId = pod.Id;
                        result.AlertStates.Created = DateTimeOffset.UtcNow;
                        conn.InsertOrReplace(result.AlertStates, typeof(ErosAlertStates));
                        result.AlertStatesId = result.AlertStates.Id;
                        pod.LastAlertStates = result.AlertStates;
                    }

                    if (result.Success && result.BasalSchedule != null)
                    {
                        result.BasalSchedule.PodId = pod.Id;
                        result.BasalSchedule.Created = DateTimeOffset.UtcNow;
                        conn.InsertOrReplace(result.BasalSchedule, typeof(ErosBasalSchedule));
                        result.BasalScheduleId = result.BasalSchedule.Id;
                        pod.LastBasalSchedule = result.BasalSchedule;
                    }

                    if (result.Success && result.Fault != null)
                    {
                        result.Fault.PodId = pod.Id;
                        result.Fault.Created = DateTimeOffset.UtcNow;
                        conn.InsertOrReplace(result.Fault, typeof(ErosFault));
                        result.FaultId = result.Fault.Id;
                        pod.LastFault = result.Fault;
                    }

                    if (result.Success && result.Status != null)
                    {
                        result.Status.PodId = pod.Id;
                        result.Status.Created = DateTimeOffset.UtcNow;
                        conn.InsertOrReplace(result.Status, typeof(ErosStatus));
                        result.StatusId = result.Status.Id;
                        pod.LastStatus = result.Status;
                        MessagingCenter.Send(result.Status, MessagingConstants.PodStatusUpdated);
                    }

                    if (result.Success && result.UserSettings != null)
                    {
                        result.UserSettings.PodId = pod.Id;
                        result.UserSettings.Created = DateTimeOffset.UtcNow;
                        conn.InsertOrReplace(result.UserSettings, typeof(ErosUserSettings));
                        result.UserSettingsId = result.UserSettings.Id;
                        pod.LastUserSettings = result.UserSettings;
                    }

                    result.PodId = pod.Id;
                    var newResult = !result.Id.HasValue;
                    conn.InsertOrReplace(result, typeof(ErosMessageExchangeResult));
                    if (newResult)
                        MessagingCenter.Send(result, MessagingConstants.NewResultReceived);
                }

                conn.Commit();
            }
        }

        public async Task<List<ErosMessageExchangeResult>> GetHistoricalResultsForDisplay(int maxCount)
        {
            var conn = new SQLiteAsyncConnection(DbPath);
            try
            {
                return await WithStatistics(
                    await conn.QueryAsync<ErosMessageExchangeResult>("SELECT * FROM ErosMessageExchangeResult ORDER BY Id DESC LIMIT ?", maxCount)
                    , conn);
            }
            finally
            {
                await conn?.CloseAsync();
            }
        }

        private async Task<List<ErosMessageExchangeResult>> WithStatistics(List<ErosMessageExchangeResult> list,
            SQLiteAsyncConnection conn)
        {
            if (list != null)
            {
                foreach (var result in list)
                {
                    if (result.StatusId.HasValue)
                        result.Status = await conn.Table<ErosStatus>().FirstOrDefaultAsync(x => x.Id == result.StatusId.Value);

                    if (result.StatisticsId.HasValue)
                        result.Statistics = await conn.Table<ErosMessageExchangeStatistics>().FirstOrDefaultAsync(x => x.Id == result.StatisticsId.Value);
                }
            }
            return list;
        }

        public async Task<List<ErosMessageExchangeResult>> GetHistoricalResultsForRemoteApp(long lastResultDate)
        {
            var conn = new SQLiteAsyncConnection(DbPath);
            try
            {
                long lastId = 0;
                var dtLastResult = DateTimeOffset.FromUnixTimeMilliseconds(lastResultDate);
                var dtNow = DateTimeOffset.UtcNow;
                if ((dtNow - dtLastResult).TotalDays > 1)
                    dtLastResult = dtNow.AddDays(-1);
                var correspondingResults = await conn.QueryAsync<ErosMessageExchangeResult>(
                    "SELECT * FROM ErosMessageExchangeResult WHERE Success <> 0 AND ResultTime <= ? ORDER BY ResultTime DESC LIMIT 1", dtLastResult.Ticks);

                if (correspondingResults != null && correspondingResults.Count > 0)
                {
                    lastId = correspondingResults[0].Id.Value;
                }

                return await WithHistoricalRelations(await conn.Table<ErosMessageExchangeResult>()
                    .Where(x => x.Success && x.Id > lastId)
                    .OrderBy(x => x.Id).ToListAsync(), conn);
            }
            finally
            {
                await conn?.CloseAsync();
            }
        }

        private async Task<List<ErosMessageExchangeResult>> WithHistoricalRelations(List<ErosMessageExchangeResult> listResults,
            SQLiteAsyncConnection conn)
        {
            if (listResults == null)
                return null;

            var list = new List<ErosMessageExchangeResult>();
            foreach(var result in listResults)
            {
                if (result.Type == RequestType.CancelBolus)
                {
                    var bolusEntry = await conn.Table<ErosMessageExchangeResult>()
                        .Where(x => x.Success && x.Type == RequestType.Bolus && x.Id < result.Id)
                        .OrderByDescending(x => x.Id).FirstOrDefaultAsync();

                    if (bolusEntry != null)
                    {
                        list.Add(await WithRelations(bolusEntry, conn));
                    }
                }
                list.Add(await WithRelations(result, conn));
            }
            return list.OrderBy(x => x.Id).ToList();
        }

        private async Task<ErosMessageExchangeResult> WithRelations(ErosMessageExchangeResult result, SQLiteAsyncConnection conn)
        {
            if (result.StatusId.HasValue)
                result.Status = await conn.Table<ErosStatus>().FirstOrDefaultAsync(x => x.Id == result.StatusId.Value);

            if (result.BasalScheduleId.HasValue)
                result.BasalSchedule = await conn.Table<ErosBasalSchedule>().FirstOrDefaultAsync(x => x.Id == result.BasalScheduleId.Value);

            if (result.FaultId.HasValue)
                result.Fault = await conn.Table<ErosFault>().FirstOrDefaultAsync(x => x.Id == result.FaultId.Value);

            return result;
        }


        private ErosPod WithRelations(ErosPod pod, SQLiteConnection conn)
        {
            if (pod == null)
                return null;

            var tempBasal = conn.Table<ErosMessageExchangeResult>()
                .Where(x => x.PodId == pod.Id && x.Success && x.Type == RequestType.SetTempBasal)
                .OrderByDescending(x => x.Id)
                .FirstOrDefault();

            var tempBasalCancel = conn.Table<ErosMessageExchangeResult>()
                .Where(x => x.PodId == pod.Id && x.Success && x.Type == RequestType.CancelTempBasal)
                .OrderByDescending(x => x.Id)
                .FirstOrDefault();

            pod.LastTempBasalResult = null;
            if (tempBasal != null &&
                (tempBasalCancel == null || tempBasalCancel.Id < tempBasal.Id))
            {
                pod.LastTempBasalResult = tempBasal;
            }

            pod.LastAlertStates = conn.Table<ErosAlertStates>().Where(x => x.PodId == pod.Id)
                .OrderByDescending(x => x.Id)
                .FirstOrDefault();

            pod.LastBasalSchedule = conn.Table<ErosBasalSchedule>().Where(x => x.PodId == pod.Id)
                .OrderByDescending(x => x.Id)
                .FirstOrDefault();

            pod.LastFault = conn.Table<ErosFault>().Where(x => x.PodId == pod.Id)
                .OrderByDescending(x => x.Id)
                .FirstOrDefault();

            pod.LastStatus = conn.Table<ErosStatus>().Where(x => x.PodId == pod.Id)
                .OrderByDescending(x => x.Id)
                .FirstOrDefault();

            pod.LastUserSettings = conn.Table<ErosUserSettings>().Where(x => x.PodId == pod.Id)
                .OrderByDescending(x => x.Id)
                .FirstOrDefault();

            return pod;
        }
    }
}
