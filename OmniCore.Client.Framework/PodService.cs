using System.Collections.Concurrent;
using System.Diagnostics;
using Microsoft.VisualStudio.Threading;
using Nito.AsyncEx;
using OmniCore.Client.Model;
using OmniCore.Common.Amqp;
using OmniCore.Common.Core;
using OmniCore.Common.Pod;
using OmniCore.Framework.Omnipod;
using OmniCore.Shared.Enums;

namespace OmniCore.Framework;

public class PodService : IPodService
{
    private readonly IAmqpService _amqpService;
    private readonly IAppConfiguration _appConfiguration;
    private readonly IRadioService _radioService;
    private readonly ISyncService _syncService;

    private ConcurrentDictionary<Guid, AsyncLock> _podLocks;
    private List<IPodModel> _podModels;

    public PodService(
        IRadioService radioService,
        IAppConfiguration appConfiguration,
        ISyncService syncService,
        IAmqpService amqpService)
    {
        _radioService = radioService;
        _appConfiguration = appConfiguration;
        _syncService = syncService;
        _amqpService = amqpService;
    }

    public async Task<List<IPodModel>> GetPodsAsync(Guid? profileId)
    {
        if (profileId == null)
            return _podModels;

        //TODO: profile filter
        return _podModels;
    }

    public async Task RemovePodAsync(Guid podId, DateTimeOffset? removeTime = null)
    {
        if (!removeTime.HasValue)
            removeTime = DateTimeOffset.UtcNow;

        if (_podLocks.ContainsKey(podId))
        {
            _podLocks.Remove(podId, out _);
            var pm = _podModels.First(p => p.Id == podId);
            _podModels.Remove(pm);
        }

        using var ocdb = new OcDbContext();
        var pod = ocdb.Pods
            .First(p => p.PodId == podId);
        pod.Removed = removeTime;
        pod.IsSynced = false;
        await ocdb.SaveChangesAsync();
        _syncService.TriggerSync();
    }

    public async Task<Guid> NewPodAsync(
        Guid profileId,
        int unitsPerMilliliter,
        MedicationType medicationType)
    {
        if (_appConfiguration.ClientAuthorization == null)
            throw new ApplicationException("Client not registered");

        using var ocdb = new OcDbContext();

        var b = new byte[4];
        new Random().NextBytes(b);
        var pod = new Pod
        {
            PodId = Guid.NewGuid(),
            ClientId = _appConfiguration.ClientAuthorization.ClientId,
            ProfileId = profileId,
            RadioAddress = (uint)((b[0] << 24) | (b[1] << 16) | (b[2] << 8) | b[3]),
            UnitsPerMilliliter = unitsPerMilliliter,
            Medication = medicationType
        };
        ocdb.Pods.Add(pod);
        await ocdb.SaveChangesAsync();

        var pm = new PodModel(pod);
        _podLocks.TryAdd(pod.PodId, new AsyncLock());
        _podModels.Add(pm);
        _syncService.TriggerSync();
        return pod.PodId;
    }

    public async Task ImportPodAsync(
        Guid profileId,
        uint radioAddress, int unitsPerMilliliter,
        MedicationType medicationType,
        uint lot,
        uint serial
    )
    {
        if (_appConfiguration.ClientAuthorization == null)
            throw new ApplicationException("Client not registered");

        using var ocdb = new OcDbContext();
        if (ocdb.Pods.Where(p => p.Lot == lot && p.Serial == serial).Any())
            return;

        var pod = new Pod
        {
            PodId = Guid.NewGuid(),
            ClientId = _appConfiguration.ClientAuthorization.ClientId,
            ProfileId = profileId,
            RadioAddress = radioAddress,
            UnitsPerMilliliter = unitsPerMilliliter,
            Medication = medicationType,
            Lot = lot,
            Serial = serial
        };
        ocdb.Pods.Add(pod);
        await ocdb.SaveChangesAsync();
        _syncService.TriggerSync();
        var pm = new PodModel(pod);
        _podLocks.TryAdd(pod.PodId, new AsyncLock());
        _podModels.Add(pm);
    }

    public async Task<IPodModel?> GetPodAsync(Guid podId)
    {
        return _podModels.FirstOrDefault(p => p.Id == podId);
    }

    public async Task<IPodConnection> GetConnectionAsync(
        IPodModel podModel,
        CancellationToken cancellationToken = default)
    {
        if (_appConfiguration.ClientAuthorization == null)
            throw new ApplicationException("Client not registered");

        var radioConnection = await _radioService.GetIdealConnectionAsync(cancellationToken);
        if (radioConnection == null)
            throw new ApplicationException("No radios available");

        var allocationLockDisposable = await _podLocks[podModel.Id].LockAsync(cancellationToken);
        var clientId = _appConfiguration.ClientAuthorization.ClientId;

        return new PodConnection(
            clientId,
            podModel,
            radioConnection,
            allocationLockDisposable,
            _syncService);
    }

    public async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _amqpService.RegisterMessageHandler(HandleMessageAsync);
        await stoppingToken.WaitHandle;
    }

    private async Task<bool> HandleMessageAsync(AmqpMessage message)
    {
        return false;
    }

    public async Task Start()
    {
        // TODO: fix starting
        try
        {
            using var ocdb = new OcDbContext();
            _podLocks = new ConcurrentDictionary<Guid, AsyncLock>();
            _podModels = new List<IPodModel>();
            var pods = ocdb.Pods
                .Where(p => !p.Removed.HasValue).ToList()
                .OrderByDescending(p => p.Created);
            foreach (var pod in pods)
            {
                // if (pod.Created < DateTimeOffset.UtcNow - TimeSpan.FromHours(82))
                //     continue;

                var pm = new PodModel(pod);
                await pm.LoadAsync();

                // if (pm.ProgressModel?.Progress == PodProgress.Deactivated)
                //     continue;

                // if (pm.Activated != null)
                // {
                //     if (pm.Activated < DateTimeOffset.UtcNow - TimeSpan.FromHours(82))
                //         continue;
                // }

                Debug.WriteLine($"Adding PodId: {pod.PodId} Created: {pod.Created}");
                _podLocks.TryAdd(pod.PodId, new AsyncLock());
                _podModels.Add(pm);
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex);
        }
    }

    public async Task Stop()
    {
        // foreach (var podLock in _podLocks.Values)
        // {
        //     await podLock.LockAsync();
        // }
    }
}