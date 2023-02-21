using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using OmniCore.Services.Entities;
using Xamarin.Forms;

namespace OmniCore.Services
{
    public class XDripWebServiceClient
    {
        [Unity.Dependency]
        public ConfigurationStore ConfigurationStore { get; set; }
        
        [Unity.Dependency]
        public BgcService BgcService { get; set; }
        
        private HttpClient _httpClient = new HttpClient()
        {
            BaseAddress = new Uri("http://127.0.0.1:17580")
        };
        
        private Guid _profileId;
        private Guid _clientId;
        private Task _xdwsTask;
        private CancellationTokenSource _cts;

        public async Task StartCollectionAsync()
        {
            var cc = await ConfigurationStore.GetConfigurationAsync();
            _clientId = cc.ClientId.Value;
            _profileId = cc.ReceiverProfileId.Value;
            _cts = new CancellationTokenSource();
            if (_xdwsTask != null)
                throw new ApplicationException("Xdrip Ws Client already started");

            _xdwsTask = Task.Run(async () => 
            {
                var latestReadingDate = await BgcService.GetLastReadingDateAsync(_profileId, cc.ClientId.Value);
                bool newArrival = false;
                bool hasGaps = false;
                TimeSpan waitInterval = TimeSpan.FromSeconds(5);
                while (true)
                {
                    if (latestReadingDate != null)
                        waitInterval = DexcomUtil.GetRefreshInterval(latestReadingDate.Value, hasGaps);
                    Debug.WriteLine($"Xdrip ws wait interval: {waitInterval}");
                    
                    await Task.Delay(waitInterval, _cts.Token);
                    
                    var readings = await GetReadingsAsync(_profileId, cc.ClientId.Value);
                    await BgcService.AddReadingsAsync(readings);
                    var sortedReadings = readings.Where(r => r.Type == BgcReadingType.CGM)
                        .OrderByDescending(r => r.Date);

                    var lastReading = sortedReadings.FirstOrDefault();
                    if (lastReading != null)
                    {
                        if (!latestReadingDate.HasValue || latestReadingDate < lastReading.Date)
                        {
                            Debug.WriteLine($"New Reading detected for timer: {lastReading.Date} {lastReading.Value}");
                            latestReadingDate = lastReading.Date;
                        }
                        else
                            Debug.WriteLine($"no new readings");
                        hasGaps = false;
                        var readingCount = sortedReadings.Count();
                        if (readingCount > 1)
                        {
                            var diffs = sortedReadings.Take(sortedReadings.Count() - 1).Zip(
                                sortedReadings.Skip(1),
                                (d0, d1) => d0.Date - d1.Date)
                                .Take(readingCount > 2 ? 2 : readingCount - 1);
                            hasGaps = diffs.Any(d => d.TotalSeconds > 450);
                        }
                    }
                }
            }, _cts.Token);
        }

        public async Task StopCollectionAsync()
        {
            if (_xdwsTask != null)
                throw new ApplicationException("Xdrip Ws is not started");
            _cts.Cancel(true);
            try
            {
                await _xdwsTask;
            }
            catch (OperationCanceledException)
            {
            }
            finally
            {
                _cts.Dispose();
                _cts = null;
            }
        }
        
        private async Task<List<BgcEntry>> GetReadingsAsync(Guid profileId, Guid clientId)
        {
            var result = await _httpClient.GetAsync(new Uri("/sgv.json"));
            string resultContent = await result.Content.ReadAsStringAsync();
            var document = JsonDocument.Parse(resultContent);
            var readings = new List<BgcEntry>();
            foreach (var element in document.RootElement.EnumerateArray())
            {
                var reading = GetBgcReadingFromElement(element);
                if (reading != null)
                {
                    reading.ProfileId = profileId;
                    reading.ClientId = clientId;
                    readings.Add(reading);
                }
            }
            return readings;
        }

        private BgcEntry GetBgcReadingFromElement(JsonElement element)
        {
            //[{"_id":"83c70e6c-5a81-4396-9144-0340051a8aa6",
            //"device":"Follower",
            //"dateString":"2022-02-06T22:20:49.892+0100",
            //"sysTime":"2022-02-06T22:20:49.892+0100",
            //"date":1644182449892,
            //"sgv":94,
            //"delta":-1,
            //"direction":"Flat",
            //"noise":1,
            //"filtered":0,
            //"unfiltered":-127,
            //"rssi":100,
            //"type":"sgv",
            //"units_hint":"mmol"}]

            var type = GetReadingType(element.GetProperty("type").GetString());
            if (!type.HasValue)
                return null;
            var udate = element.GetProperty("date").GetInt64();
            var date = DateTimeOffset.FromUnixTimeMilliseconds(udate);
            var direction = DexcomUtil.GetDirection(element.GetProperty("direction").GetString());
            double value;
            if (type == BgcReadingType.Manual)
                value = element.GetProperty("mbg").GetDouble();
            else
                value = element.GetProperty("sgv").GetDouble();
            int? rssi = element.GetProperty("rssi").GetInt32();
            return
                new BgcEntry()
                {
                    Date = date,
                    Direction = direction,
                    Type = type,
                    Value = value,
                    Rssi = rssi,
                };
        }

        private BgcReadingType? GetReadingType(string type)
        {
            var t = type.ToLowerInvariant();
            if (t == "sgv")
                return BgcReadingType.CGM;
            if (t == "mbg")
                return BgcReadingType.Manual;
            return null;
        }
    }
}