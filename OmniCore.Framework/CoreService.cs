using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using OmniCore.Common.Data;
using OmniCore.Services.Interfaces;
using OmniCore.Services.Interfaces.Core;
using OmniCore.Services.Interfaces.Pod;
using static OmniCore.Common.Data.OcdbContext;

namespace OmniCore.Services;

public class CoreService : ICoreService
{
    public IRadioService RadioService { get; set; }
    public IAmqpService AmqpService { get; set; }
    public IPodService PodService { get; set; }
    public ISyncService SyncService { get; set; }
    public IRaddService RaddService { get; set; }
    public CoreService(IRadioService radioService,
        IAmqpService amqpService,
        IPodService podService,
        ISyncService syncService,
        IRaddService raddService)
    {
        RadioService = radioService;
        AmqpService = amqpService;
        PodService = podService;
        SyncService = syncService;
        RaddService = raddService;
    }

    public async Task Start()
    {
        Debug.WriteLine("Core services starting");

        try
        {
            //await TransferDb();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"{ex.Message}");
        }

        await RadioService.Start();
        await PodService.Start();
        await SyncService.Start();
        await RaddService.Start();
        await AmqpService.Start();
        Debug.WriteLine("Core services started");
    }

    public async Task Stop()
    {
        Debug.WriteLine("Core services stopping");
        await AmqpService.Stop();
        await RaddService.Stop();
        await SyncService.Stop();
        await PodService.Stop();
        await RadioService.Stop();
        Debug.WriteLine("Core services stopped");
    }
    //
    // private async Task TransferDb()
    // {
    //     var basePath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
    //     var databasePath = Path.Combine(basePath, "omnicore.db3");
    //
    //
    //     var accId = Guid.Parse("269d7830-fe9b-4641-8123-931846e45c9c");
    //     var clientId = Guid.Parse("ee843c96-a312-4d4b-b0cc-93e22d6e680e");
    //     var profileId = Guid.Parse("7d799596-3f6d-48e2-ac65-33ca6396788b");
    //
    //     using (var context = new OcdbContext())
    //     {
    //         Debug.WriteLine("deleting");
    //         await context.Database.EnsureDeletedAsync();
    //         Debug.WriteLine("creating");
    //         await context.Database.EnsureCreatedAsync();
    //
    //         context.Accounts.Add(new Account
    //         {
    //             AccountId = accId,
    //             IsSynced = true
    //         });
    //
    //         context.Clients.Add(new Client
    //         {
    //             ClientId = clientId,
    //             AccountId = accId,
    //             Name = "luks",
    //             IsSynced = true
    //         });
    //
    //         context.Profiles.Add(new Profile
    //         {
    //             ProfileId = profileId,
    //             AccountId = accId,
    //             Name = "lukluk",
    //             IsSynced = true
    //         });
    //         await context.SaveChangesAsync();
    //     }
    //
    //     Debug.WriteLine($"select pods");
    //     await using var oldConn = new SqliteConnection($"Data Source={databasePath}");
    //     await oldConn.OpenAsync();
    //
    //     var oPods = await oldConn.QueryAsync("SELECT * FROM pod");
    //
    //     foreach (var oPod in oPods)
    //     {
    //         using (var context = new OcdbContext())
    //         {
    //             context.Pods.Add(new Pod
    //             {
    //                 PodId = Guid.Parse((string)oPod.id),
    //                 ProfileId = profileId,
    //                 ClientId = clientId,
    //                 RadioAddress = (uint)oPod.radio_address,
    //                 UnitsPerMilliliter = (int)oPod.units_per_ml,
    //                 Medication = (MedicationType)oPod.medication,
    //                 Created = DateTimeOffset.FromUnixTimeMilliseconds(oPod.valid_from),
    //                 Lot = (uint)oPod.assumed_lot,
    //                 Serial = (uint)oPod.assumed_serial,
    //                 IsSynced = false
    //             });
    //             Debug.WriteLine($"pod: {oPod.id}");
    //
    //             var oMessages = await oldConn.QueryAsync("SELECT * FROM pod_message WHERE pod_id = @podId",
    //                 new { podId = (string)oPod.id });
    //
    //             foreach (var oMsg in oMessages)
    //             {
    //                 context.PodActions.Add(new PodAction
    //                 {
    //                     PodId = Guid.Parse((string)oMsg.pod_id),
    //                     Index = (int)oMsg.record_index,
    //                     ClientId = clientId,
    //                     Result = AcceptanceType.Inconclusive,
    //                     RequestSentEarliest = DateTimeOffset.FromUnixTimeMilliseconds((long)oMsg.send_start),
    //                     RequestSentLatest = DateTimeOffset.FromUnixTimeMilliseconds((long)oMsg.receive_end),
    //                     SentData = (byte[])oMsg.send_data,
    //                     ReceivedData = (byte[]?)oMsg.receive_data,
    //                     IsSynced = false
    //                 });
    //                 Debug.WriteLine($"msg: {oMsg.pod_id} {oMsg.record_index}");
    //             }
    //             Debug.WriteLine("saving");
    //             await context.SaveChangesAsync();
    //             Debug.WriteLine("saved");
    //         }
    //     }
    //
    //     Debug.WriteLine("done migrating");
    // }
}
