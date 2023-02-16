using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dapper;

namespace OmniCore.Services;

public class PodService
{
    private RadioService _radioService;
    private DataStore _dataStore;
    public PodService(RadioService radioService, DataStore dataStore)
    {
        _radioService = radioService;
        _dataStore = dataStore;
    }

    public async Task<Pod> GetPodAsync()
    {
        Pod pod = null;
        using (var conn = await _dataStore.GetConnectionAsync())
        {
            var row = await conn.QueryFirstOrDefaultAsync("SELECT * FROM pod");
            if (row == null)
            {
                pod = new Pod(_dataStore)
                {
                    RadioAddress = 878987447,
                    Lot = 72402,
                    Serial = 3220596,
                    NextMessageSequence = 0,
                    NextPacketSequence = 0,
                    UnitsPerMilliliter = 200,
                    Medication = MedicationType.Insulin,
                    Progress = PodProgress.Running,
                    Entered = DateTimeOffset.Now,
                    Removed = null,
                };
                await conn.ExecuteAsync(
                    "INSERT INTO pod(id, radio_address, units_per_ml, medication, lot, serial, progress," +
                    " packet_sequence, message_sequence, entered)" +
                    " VALUES (@id, @ra, @upml, @med, @lot, @serial, @pro, @ps, @ms, @entered)",
                    new
                    {
                        id = pod.Id.ToString("N"),
                        ra = pod.RadioAddress,
                        upml = pod.UnitsPerMilliliter,
                        med = (int)pod.Medication,
                        lot = pod.Lot,
                        serial = pod.Serial,
                        pro = (int)pod.Progress,
                        ps = pod.NextPacketSequence,
                        ms = pod.NextMessageSequence,
                        entered = pod.Entered.ToUnixTimeMilliseconds(),
                    });
            }
            else
            {
                pod = new Pod(_dataStore)
                {
                    Id = Guid.Parse(row.id),
                    RadioAddress = (uint)row.radio_address,
                    UnitsPerMilliliter = (int)row.units_per_ml,
                    Medication = (MedicationType)row.medication,
                    Lot = (uint)row.lot,
                    Serial = (uint)row.serial,
                    Progress = (PodProgress)row.progress,
                    NextPacketSequence = (int)row.packet_sequence,
                    NextMessageSequence = (int)row.message_sequence,
                    Entered = DateTimeOffset.FromUnixTimeMilliseconds(row.entered),
                };
            }
        }
        return pod;
    }
    
    public async Task<PodConnection> GetConnectionAsync(
        Pod pod,
        CancellationToken cancellationToken = default)
    {
        var radioConnection = await _radioService.GetConnectionAsync("ema");
        var podAllocationLockDisposable = await pod.LockAsync(cancellationToken);
        return new PodConnection(pod, radioConnection, podAllocationLockDisposable);
    }
}