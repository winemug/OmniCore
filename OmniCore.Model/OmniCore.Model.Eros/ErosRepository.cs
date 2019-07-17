using OmniCore.Model.Interfaces;
using SQLite;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OmniCore.Model.Eros.Data;
using OmniCore.Model.Enums;
using OmniCore.Mobile.Base;
using OmniCore.Model.Data;
using Xamarin.Forms;
using Microsoft.AppCenter.Crashes;

namespace OmniCore.Model.Eros
{
    public class ErosRepository : IDisposable
    {
        private static ErosRepository instance = null;
        public static async Task<ErosRepository> GetInstance()
        {
            if (instance == null)
            {
                instance = new ErosRepository();
                await instance.Initialize();
            }
            return instance;
        }

        public readonly string DbPath;

        private SQLiteAsyncConnection Connection;

        private ErosRepository()
        {
            DbPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "omnicore.db3");
        }

        private bool IsInitialized = false;
        public async Task Initialize()
        {
            if (IsInitialized)
                return;

            Connection = new SQLiteAsyncConnection(DbPath, SQLiteOpenFlags.Create | SQLiteOpenFlags.ReadWrite
                | SQLiteOpenFlags.FullMutex);
            try
            {
                await Connection.RunInTransactionAsync( (conn) =>
                {
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

                    if (!conn.Table<ErosProfile>().Any())
                    {
                        conn.Insert(new ErosProfile()
                        {
                            Created = DateTimeOffset.UtcNow,
                            Name = "Default",
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

                    if (!conn.Table<ErosRadioPreferences>().Any())
                    {
                        conn.Insert(new ErosRadioPreferences()
                        {
                            ConnectToAny = true
                        });
                    }

                    if (!conn.Table<OmniCoreSettings>().Any())
                    {
                        conn.Insert(new OmniCoreSettings()
                        {
                            AcceptCommandsFromAAPS = true
                        });
                    }

                });

                IsInitialized = true;
            }
            catch (SQLiteException sle)
            {
                Crashes.TrackError(sle);
                throw sle;
            }
        }

        public async Task<List<ErosPod>> GetActivePods()
        {
            return await Connection.Table<ErosPod>()
                .Where(x => !x.Archived)
                .OrderByDescending(x => x.Created)
                .ToListAsync();
        }

        public async Task<ErosPod> LoadCurrent()
        {
            return await WithRelations(await Connection.Table<ErosPod>()
                .Where(x => !x.Archived)
                .OrderByDescending(x => x.Created)
                .FirstOrDefaultAsync());
        }

        public async Task<ErosPod> Load(uint lot, uint tid)
        {
            return await WithRelations(await Connection.Table<ErosPod>()
                .FirstOrDefaultAsync(x => x.Lot == lot && x.Serial == tid));
        }

        public async Task<ErosRadioPreferences> GetRadioPreferences()
        {
            return await Connection.Table<ErosRadioPreferences>().FirstOrDefaultAsync();
        }

        public async Task<OmniCoreSettings> GetOmniCoreSettings()
        {
            return await Connection.Table<OmniCoreSettings>().FirstOrDefaultAsync();
        }

        public async Task SaveOmniCoreSettings(OmniCoreSettings settings)
        {
            await Connection.UpdateAsync(settings);
        }

        public async Task<ErosPod> GetLastActivated()
        {
            return await WithRelations(await Connection.Table<ErosPod>().OrderByDescending(x => x.ActivationDate.Value.Ticks)
                .FirstOrDefaultAsync());
        }

        public async Task<IProfile> GetProfile()
        {
            return await Connection.Table<ErosProfile>()
                .OrderByDescending(x => x.Id)
                .FirstOrDefaultAsync();
        }

        public async Task Save(IProfile profile)
        {
            await Connection.InsertOrReplaceAsync(profile, typeof(ErosProfile));
        }

        public async Task Save(IPod pod, IMessageExchange exchange = null)
        {
            await Connection.RunInTransactionAsync((conn) =>
            {
                conn.InsertOrReplace(pod);

                if (exchange != null)
                {
                    var result = exchange.Result;

                    if (exchange.Statistics != null)
                    {
                        exchange.Statistics.PodId = pod.Id;
                        exchange.Statistics.Created = DateTimeOffset.UtcNow;
                        exchange.Statistics.BeforeSave();
                        conn.InsertOrReplace(exchange.Statistics, typeof(ErosMessageExchangeStatistics));
                        result.StatisticsId = exchange.Statistics.Id;
                    }

                    if (exchange.Parameters != null)
                    {
                        exchange.Parameters.PodId = pod.Id;
                        exchange.Parameters.Created = DateTimeOffset.UtcNow;
                        conn.InsertOrReplace(exchange.Parameters, typeof(ErosMessageExchangeParameters));
                        result.ParametersId = exchange.Parameters.Id;
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
            });
        }

        public async Task<List<ErosMessageExchangeResult>> GetHistoricalResultsForDisplay(int maxCount)
        {
            return await WithStatistics(
                await Connection.QueryAsync<ErosMessageExchangeResult>("SELECT * FROM ErosMessageExchangeResult ORDER BY Id DESC LIMIT ?", maxCount));
        }

        private async Task<List<ErosMessageExchangeResult>> WithStatistics(List<ErosMessageExchangeResult> list)
        {
            if (list != null)
            {
                foreach (var result in list)
                {
                    if (result.StatusId.HasValue)
                        result.Status = await Connection.Table<ErosStatus>().FirstOrDefaultAsync(x => x.Id == result.StatusId.Value);

                    if (result.StatisticsId.HasValue)
                        result.Statistics = await Connection.Table<ErosMessageExchangeStatistics>().FirstOrDefaultAsync(x => x.Id == result.StatisticsId.Value);
                }
            }
            return list;
        }

        public async Task<List<ErosMessageExchangeResult>> GetHistoricalResultsForRemoteApp(long lastResultDate)
        {
            long lastId = 0;
            var dtLastResult = DateTimeOffset.FromUnixTimeMilliseconds(lastResultDate);
            var dtNow = DateTimeOffset.UtcNow;
            if ((dtNow - dtLastResult).TotalDays > 1)
                dtLastResult = dtNow.AddDays(-1);
            var correspondingResults = await Connection.QueryAsync<ErosMessageExchangeResult>(
                "SELECT * FROM ErosMessageExchangeResult WHERE Success <> 0 AND ResultTime <= ? ORDER BY ResultTime DESC LIMIT 1", dtLastResult.Ticks);

            if (correspondingResults != null && correspondingResults.Count > 0)
            {
                lastId = correspondingResults[0].Id.Value;
            }

            return await WithHistoricalRelations(await Connection.Table<ErosMessageExchangeResult>()
                .Where(x => x.Success && x.Id > lastId)
                .OrderBy(x => x.Id).ToListAsync());
        }

        private async Task<List<ErosMessageExchangeResult>> WithHistoricalRelations(List<ErosMessageExchangeResult> listResults)
        {
            if (listResults == null)
                return null;

            var list = new List<ErosMessageExchangeResult>();
            foreach(var result in listResults)
            {
                if (result.Type == RequestType.CancelBolus)
                {
                    var bolusEntry = await Connection.Table<ErosMessageExchangeResult>()
                        .Where(x => x.Success && x.Type == RequestType.Bolus && x.Id < result.Id)
                        .OrderByDescending(x => x.Id).FirstOrDefaultAsync();

                    if (bolusEntry != null)
                    {
                        list.Add(await WithRelations(bolusEntry));
                    }
                }
                list.Add(await WithRelations(result));
            }
            return list.OrderBy(x => x.Id).ToList();
        }

        private async Task<ErosMessageExchangeResult> WithRelations(ErosMessageExchangeResult result)
        {
            if (result.StatusId.HasValue)
                result.Status = await Connection.Table<ErosStatus>().FirstOrDefaultAsync(x => x.Id == result.StatusId.Value);

            if (result.BasalScheduleId.HasValue)
                result.BasalSchedule = await Connection.Table<ErosBasalSchedule>().FirstOrDefaultAsync(x => x.Id == result.BasalScheduleId.Value);

            if (result.FaultId.HasValue)
                result.Fault = await Connection.Table<ErosFault>().FirstOrDefaultAsync(x => x.Id == result.FaultId.Value);

            return result;
        }


        private async Task<ErosPod> WithRelations(ErosPod pod)
        {
            if (pod == null)
                return null;

            var tempBasal = await Connection.Table<ErosMessageExchangeResult>()
                .Where(x => x.PodId == pod.Id && x.Success && x.Type == RequestType.SetTempBasal)
                .OrderByDescending(x => x.Id)
                .FirstOrDefaultAsync();

            var tempBasalCancel = await Connection.Table<ErosMessageExchangeResult>()
                .Where(x => x.PodId == pod.Id && x.Success && x.Type == RequestType.CancelTempBasal)
                .OrderByDescending(x => x.Id)
                .FirstOrDefaultAsync();

            pod.LastTempBasalResult = null;
            if (tempBasal != null &&
                (tempBasalCancel == null || tempBasalCancel.Id < tempBasal.Id))
            {
                pod.LastTempBasalResult = tempBasal;
            }

            pod.LastAlertStates = await Connection.Table<ErosAlertStates>().Where(x => x.PodId == pod.Id)
                .OrderByDescending(x => x.Id)
                .FirstOrDefaultAsync();

            pod.LastBasalSchedule = await Connection.Table<ErosBasalSchedule>().Where(x => x.PodId == pod.Id)
                .OrderByDescending(x => x.Id)
                .FirstOrDefaultAsync();

            pod.LastFault = await Connection.Table<ErosFault>().Where(x => x.PodId == pod.Id)
                .OrderByDescending(x => x.Id)
                .FirstOrDefaultAsync();

            pod.LastStatus = await Connection.Table<ErosStatus>().Where(x => x.PodId == pod.Id)
                .OrderByDescending(x => x.Id)
                .FirstOrDefaultAsync();

            pod.LastUserSettings = await Connection.Table<ErosUserSettings>().Where(x => x.PodId == pod.Id)
                .OrderByDescending(x => x.Id)
                .FirstOrDefaultAsync();

            return pod;
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected async virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                    if (Connection != null)
                    {
                        await Connection.CloseAsync();
                        Connection = null;
                        instance = null;
                    }
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~ErosRepository()
        // {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion
    }
}
