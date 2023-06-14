using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Nito.AsyncEx;
using OmniCore.Common.Data;
using OmniCore.Services.Interfaces;
using OmniCore.Services.Interfaces.Core;
using OmniCore.Services.Interfaces.Pod;
using Plugin.BLE.Abstractions;


namespace OmniCore.Services;

public class PodService : IPodService
{
    private IRadioService _radioService;
    private ISyncService _syncService;
    private IAppConfiguration _appConfiguration;
    
    private ConcurrentDictionary<Guid, AsyncLock> _podLocks;
    private List<IPodModel> _podModels;

    public PodService(
        IRadioService radioService,
        IAppConfiguration appConfiguration,
        ISyncService syncService)
    {
        _radioService = radioService;
        _appConfiguration = appConfiguration;
        _syncService = syncService;
    }

    public async Task Start()
    {
        // TODO: fix starting
        try
        {
            using var ocdb = new OcdbContext();
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
        
        using var ocdb = new OcdbContext();
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
        if (_appConfiguration.Authorization == null)
            throw new ApplicationException("Client not registered");

        using var ocdb = new OcdbContext();
        
        var b = new byte[4];
        new Random().NextBytes(b);
        var pod = new Pod
        {
            PodId = Guid.NewGuid(),
            ClientId = _appConfiguration.Authorization.ClientId,
            ProfileId = profileId,
            RadioAddress = (uint)(b[0] << 24 | b[1] << 16 | b[2] << 8 | b[3]),
            UnitsPerMilliliter = unitsPerMilliliter,
            Medication = medicationType,
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
        if (_appConfiguration.Authorization == null)
            throw new ApplicationException("Client not registered");
        
        using var ocdb = new OcdbContext();
        if (ocdb.Pods.Where(p => p.Lot == lot && p.Serial == serial).Any())
            return;

        var pod = new Pod
        {
            PodId = Guid.NewGuid(),
            ClientId = _appConfiguration.Authorization.ClientId,
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
        if (_appConfiguration.Authorization == null)
            throw new ApplicationException("Client not registered");
        
        var radioConnection = await _radioService.GetIdealConnectionAsync(cancellationToken);
        if (radioConnection == null)
            throw new ApplicationException("No radios available");

        var allocationLockDisposable = await _podLocks[podModel.Id].LockAsync(cancellationToken);
        var clientId = _appConfiguration.Authorization.ClientId;

        return new PodConnection(
            clientId,
            podModel,
            radioConnection,
            allocationLockDisposable,
            _syncService);
    }
}