using System.Diagnostics;
using Dapper;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using OmniCore.Services.Interfaces.Pod;

namespace OmniCore.Common.Data;

public class OcdbContext : DbContext
{
    public DbSet<Account> Accounts { get; set; } = null!;
    public DbSet<Client> Clients { get; set; } = null!;
    public DbSet<Profile> Profiles { get; set; } = null!;
    public DbSet<Pod> Pods { get; set; } = null!;
    public DbSet<PodAction> PodActions { get; set; } = null!;
    public string DbPath { get; }
    public OcdbContext()
    {
        var folder = Environment.SpecialFolder.LocalApplicationData;
        var path = Environment.GetFolderPath(folder);
        DbPath = Path.Join(path, "ocefcore.sqlite");
    }

    protected override void OnConfiguring(DbContextOptionsBuilder options)
    {
        options.UseSqlite($"Data Source={DbPath}");
    }
    
    public async Task TransferDb()
    {
        var basePath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        var databasePath = Path.Combine(basePath, "omnicore.db3");
        using var oldConn = new SqliteConnection($"Data Source={databasePath};Cache=Shared");
        await oldConn.OpenAsync();

        var accId = Guid.Parse("269d7830-fe9b-4641-8123-931846e45c9c");
        var clientId = Guid.Parse("ee843c96-a312-4d4b-b0cc-93e22d6e680e");
        var profileId = Guid.Parse("7d799596-3f6d-48e2-ac65-33ca6396788b");

        await this.Database.EnsureDeletedAsync();
        await this.Database.EnsureCreatedAsync();
        
        this.Accounts.Add(new Account
        {
            AccountId = accId,
            IsSynced = true
        });

        this.Clients.Add(new Client
        {
            ClientId = clientId,
            AccountId = accId,
            Name = "luks",
            IsSynced = true
        });

        this.Profiles.Add(new Profile
        {
            ProfileId = profileId,
            AccountId = accId,
            Name = "lukluk",
            IsSynced = true
        });

        Debug.WriteLine($"select pods");
        var oPods = await oldConn.QueryAsync("SELECT * FROM pod");
        Debug.WriteLine($"pods: {oPods.Count()}");
        foreach (var oPod in oPods)
        {
            this.Pods.Add(new OmniCore.Common.Data.Pod
            {
                PodId = Guid.Parse((string)oPod.id),
                ProfileId = profileId,
                ClientId = clientId,
                RadioAddress = (uint)oPod.radio_address,
                UnitsPerMilliliter = (int)oPod.units_per_ml,
                Medication = (MedicationType)oPod.medication,
                Created = DateTimeOffset.FromUnixTimeMilliseconds(oPod.valid_from),
                Lot = oPod.assumed_lot,
                Serial = oPod.assumed_serial,
                IsSynced = false
            });
            Debug.WriteLine($"pod: {oPod.id}");
        }

        Debug.WriteLine($"select messages");
        var oMessages = await oldConn.QueryAsync("SELECT * FROM pod_message");
        Debug.WriteLine($"messages: {oMessages.Count()}");
        foreach (var oMsg in oMessages)
        {
            this.PodActions.Add(new PodAction
            {
                PodId = Guid.Parse((string)oMsg.pod_id),
                Index = (int)oMsg.record_index,
                ClientId = clientId,
                SendStart = DateTimeOffset.FromUnixTimeMilliseconds((long)oMsg.send_start),
                Sent = (byte[]) oMsg.send_data,
                ReceiveEnd = DateTimeOffset.FromUnixTimeMilliseconds((long)oMsg.receive_end),
                Received = (byte[]?) oMsg.receive_data,
                Error = (CommunicationError) oMsg.exchange_result,
                IsSynced = false
            });
            Debug.WriteLine($"msg: {oMsg.pod_id} {oMsg.record_index}");
        }

        Debug.WriteLine("saving");
        await this.SaveChangesAsync();
        Debug.WriteLine("saved");
    }
}

public class Account
{
    public Guid AccountId { get; set; }
    public string? Name { get; set; }
    public string? Country { get; set; }
    public string? Phone { get; set; }
    public DateTimeOffset Created { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset LastUpdated { get; set; } = DateTimeOffset.UtcNow;
    public bool IsSynced { get; set; }
}

public class Client
{
    public Guid ClientId { get; set; }
    public Guid AccountId { get; set; }
    public string Name { get; set; }
    public DateTimeOffset Created { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset LastUpdated { get; set; } = DateTimeOffset.UtcNow;
    public bool IsDeleted { get; set; }
    public bool IsSynced { get; set; }
    public Account Account { get; set; }
}

public class Profile
{
    public Guid ProfileId { get; set; }
    public Guid AccountId { get; set; }
    public string Name { get; set; } = null!;
    public DateTimeOffset Created { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset LastUpdated { get; set; } = DateTimeOffset.UtcNow;
    public bool IsDeleted { get; set; }
    public bool IsSynced { get; set; }
    public Account Account { get; set; }
}

public class Pod
{
    public Guid PodId { get; set; }
    public Guid ProfileId { get; set; }
    public Guid ClientId { get; set; }
    public uint RadioAddress { get; set; }
    public MedicationType Medication { get; set; }
    public int UnitsPerMilliliter { get; set; }
    public uint? Lot { get; set; }
    public uint? Serial { get; set; }
    
    public bool IsImported { get; set; }
    public DateTimeOffset Created { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? Removed { get; set; }
    public DateTimeOffset LastUpdated { get; set; } = DateTimeOffset.UtcNow;
    public bool IsSynced { get; set; }
    public List<PodAction> Actions { get; set; }
}

[PrimaryKey(nameof(PodId), nameof(Index))]
public class PodAction
{
    public Guid PodId { get; set; }
    public int Index { get; set; }
    public Guid ClientId { get; set; }
    public DateTimeOffset SendStart { get; set; }
    public byte[] Sent { get; set; } = null!;
    public DateTimeOffset ReceiveEnd { get; set; }
    public byte[]? Received { get; set; } 
    public CommunicationError Error { get; set; }
    public bool IsSynced { get; set; }
}
