using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using OmniCore.Services.Interfaces;
using OmniCore.Services.Interfaces.Core;
using OmniCore.Services.Interfaces.Pod;
using Plugin.BLE.Abstractions;


namespace OmniCore.Services;

public class PodService : IPodService
{
    private IRadioService _radioService;
    private IDataService _dataService;
    private IConfigurationStore _configurationStore;
    private ISyncService _syncService;
    
    private List<IPod> _activePods = new List<IPod>();
    public PodService(IDataService dataService, IRadioService radioService,
        IConfigurationStore configurationStore, ISyncService syncService)
    {
        _dataService = dataService;
        _radioService = radioService;
        _configurationStore = configurationStore;
        _syncService = syncService;
    }

    public async Task Start()
    {
        var cc = await _configurationStore.GetConfigurationAsync();

        // await ImportPodAsync(Guid.NewGuid(), 0x1F0E89F2, 200,
        //     MedicationType.Insulin, 72402, 3460572, 12);
        
         using var conn = await _dataService.GetConnectionAsync();
         
         var rs = await conn.QueryAsync("SELECT * FROM pod");
         foreach (var r in rs)
         {
             var pod = new Pod(_dataService)
             {
                 Id = Guid.Parse(r.id),
                 RadioAddress = (uint)r.radio_address,
                 UnitsPerMilliliter = 200,
                 Medication = MedicationType.Insulin,
                 ValidFrom = DateTimeOffset.FromUnixTimeMilliseconds((long)r.valid_from),
                 ValidTo = DateTimeOffset.FromUnixTimeMilliseconds((long)r.valid_to),
                 AssumedLot = (uint)r.assumed_lot,
                 AssumedSerial = (uint)r.assumed_serial,
                 AssumedFixedBasalRate = 8
             };
             await pod.LoadResponses();
             if (pod.ValidTo > DateTimeOffset.Now && pod.Progress < PodProgress.Inactive)
                _activePods.Add(pod);
         }
    }

    public async Task Stop()
    {
    }

    public async Task<List<IPod>> GetPodsAsync()
    {
        return _activePods;
    }

    // 0x1F0E89F1
    // 72402
    // 3570557
    
    public async Task ImportPodAsync(Guid id,
        uint radioAddress, int unitsPerMilliliter,
        MedicationType medicationType,
        uint lot,
        uint serial,
        uint activeFixedBasalRateTicks
        )
    {
        var cc = await _configurationStore.GetConfigurationAsync();
        var pod = new Pod(_dataService)
        {
            Id = id,
            RadioAddress = radioAddress,
            UnitsPerMilliliter = unitsPerMilliliter,
            Medication = medicationType,
            AssumedLot = lot,
            AssumedSerial = serial,
            AssumedFixedBasalRate = activeFixedBasalRateTicks,
        };

        using(var conn = await _dataService.GetConnectionAsync())
        {
        await conn.ExecuteAsync("INSERT INTO pod(id, profile_id, client_id," +
                                "radio_address, units_per_ml, medication, valid_from, valid_to," +
                                "assumed_lot, assumed_serial, assumed_fixed_basal)" +
                                "VALUES (@id, @profileId, @clientId, @radioAddress, @unitsPerMl," +
                                "@medication, @valid_from, @valid_to, @assumedLot, @assumedSerial, @assumedFixedBasal)",
            new
            {
                id = id.ToString("N"),
                profileId = "0",
                clientId = cc.ClientId.Value.ToString("N"),
                radioAddress = radioAddress,
                unitsPerMl = unitsPerMilliliter,
                @medication = (int)medicationType,
                valid_from = DateTimeOffset.Now.ToUnixTimeMilliseconds(),
                valid_to = (DateTimeOffset.Now + TimeSpan.FromHours(88)).ToUnixTimeMilliseconds(),
                assumedLot = lot,
                assumedSerial = serial,
                assumedFixedBasal = activeFixedBasalRateTicks
            });
        }
    }
    
    public async Task<IPod?> GetPodAsync(Guid id)
    {
        return _activePods.FirstOrDefault(p => p.Id == id);
    }

    public async Task<IPodConnection> GetConnectionAsync(
        IPod pod,
        CancellationToken cancellationToken = default)
    {
        var radioConnection = await _radioService.GetIdealConnectionAsync(cancellationToken);
        if (radioConnection == null)
            throw new ApplicationException("No radios available");

        var podAllocationLockDisposable = await pod.LockAsync(cancellationToken);
        return new PodConnection(pod, radioConnection, podAllocationLockDisposable, _dataService, _configurationStore, _syncService);
    }
}