using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dapper;

namespace OmniCore.Services;

public class PodService
{
    [Unity.Dependency]
    public RadioService RadioService { get; set; }

    [Unity.Dependency]
    public DataService DataService { get; set; }

    public void Start()
    {
        
    }

    public void Stop()
    {
        
    }
    
    public async Task<Pod> GetPodAsync()
    {
        Pod pod = null;
        using (var conn = await DataService.GetConnectionAsync())
        {
            var row = await conn.QueryFirstOrDefaultAsync("SELECT * FROM pod");
            if (row == null)
            {
                pod = new Pod(DataService)
                {
                    RadioAddress = 887030921,
                    Lot = 72402,
                    Serial = 3580572,
                    NextMessageSequence = 0,
                    NextPacketSequence = 21,
                    NextRecordIndex = 0,
                    UnitsPerMilliliter = 200,
                    Medication = MedicationType.Insulin,
                    Progress = PodProgress.RunningLow,
                    ValidFrom = DateTimeOffset.Now,
                    ValidTo = DateTimeOffset.Now + TimeSpan.FromHours(80),
                };
                await conn.ExecuteAsync(
                    "INSERT INTO pod(id, profile_id, client_id, radio_address, units_per_ml, medication, lot, serial, progress," +
                    " packet_sequence, message_sequence, valid_from, valid_to)" +
                    " VALUES (@id, @profile_id, @client_id, @ra, @upml, @med, @lot, @serial, @pro, @ps, @ms, @vf, @vt)",
                    new
                    {
                        id = pod.Id.ToString("N"),
                        profile_id = "9",
                        client_id = "9",
                        ra = pod.RadioAddress,
                        upml = pod.UnitsPerMilliliter,
                        med = (int)pod.Medication,
                        lot = pod.Lot,
                        serial = pod.Serial,
                        pro = (int)pod.Progress,
                        ps = pod.NextPacketSequence,
                        ms = pod.NextMessageSequence,
                        vf = pod.ValidFrom.ToUnixTimeMilliseconds(),
                        vt = pod.ValidTo.ToUnixTimeMilliseconds(),
                    });
            }
            else
            {
                pod = new Pod(DataService)
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
                    NextRecordIndex = (int)row.next_record_index,
                    ValidFrom = DateTimeOffset.FromUnixTimeMilliseconds(row.valid_from),
                    ValidTo = DateTimeOffset.FromUnixTimeMilliseconds(row.valid_to),
                };
            }
        }
        return pod;
    }
    
    public async Task<PodConnection> GetConnectionAsync(
        Pod pod,
        CancellationToken cancellationToken = default)
    {
        var radioConnection = await RadioService.GetConnectionAsync("ema");
        var podAllocationLockDisposable = await pod.LockAsync(cancellationToken);
        return new PodConnection(pod, radioConnection, podAllocationLockDisposable, DataService);
    }
}