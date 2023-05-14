using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
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
    private IConfigurationStore _configurationStore;
    private ISyncService _syncService;
    private ConcurrentDictionary<Guid, AsyncLock> _podLocks;
    private ConcurrentBag<IPodModel> _podModels;

    public PodService(
        IRadioService radioService,
        IConfigurationStore configurationStore,
        ISyncService syncService)
    {
        _radioService = radioService;
        _configurationStore = configurationStore;
        _syncService = syncService;
    }

    public async Task Start()
    {
        try
        {
            using var ocdb = new OcdbContext();
            _podLocks = new ConcurrentDictionary<Guid, AsyncLock>();
            _podModels = new ConcurrentBag<IPodModel>();
            var pods = ocdb.Pods
                         .Where(p => !p.Removed.HasValue).ToList()
                         .OrderByDescending(p => p.Created);
            foreach (var pod in pods)
            {
                if (pod.Created < DateTimeOffset.Now - TimeSpan.FromHours(82))
                    continue;
                var pm = new PodModel(pod);
                await pm.LoadAsync();

                if (pm.ProgressModel?.Progress == PodProgress.Deactivated)
                    continue;

                if (pm.Activated != null)
                {
                    if (pm.Activated < DateTimeOffset.Now - TimeSpan.FromHours(82))
                        continue;
                }

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
        foreach (var podLock in _podLocks.Values)
        {
            await podLock.LockAsync();
        }
    }

    public async Task<List<IPodModel>> GetPodsAsync()
    {
        return _podModels.ToList();
    }
    
    public async Task ImportPodAsync(Guid id,
        uint radioAddress, int unitsPerMilliliter,
        MedicationType medicationType,
        uint lot,
        uint serial
        )
    {
        var cc = await _configurationStore.GetConfigurationAsync();
        using var ocdb = new OcdbContext();
        if (ocdb.Pods.Where(p => p.Lot == lot && p.Serial == serial).Any())
            return;

        var pod = new Pod
        {
            PodId = id,
            ClientId = cc.ClientId.Value,
            ProfileId = Guid.Parse("7d799596-3f6d-48e2-ac65-33ca6396788b"),
            RadioAddress = radioAddress,
            UnitsPerMilliliter = unitsPerMilliliter,
            Medication = medicationType,
            Lot = lot,
            Serial = serial
        };
        ocdb.Pods.Add(pod);
        await ocdb.SaveChangesAsync();
    }
    
    public async Task<IPodModel?> GetPodAsync(Guid podId)
    {
        return _podModels.FirstOrDefault(p => p.Id == podId);
    }

    public async Task<IPodConnection> GetConnectionAsync(
        IPodModel podModel,
        CancellationToken cancellationToken = default)
    {
        var radioConnection = await _radioService.GetIdealConnectionAsync(cancellationToken);
        if (radioConnection == null)
            throw new ApplicationException("No radios available");

        var allocationLockDisposable = await _podLocks[podModel.Id].LockAsync(cancellationToken);
        var accId = Guid.Parse("269d7830-fe9b-4641-8123-931846e45c9c");
        var clientId = Guid.Parse("ee843c96-a312-4d4b-b0cc-93e22d6e680e");
        var profileId = Guid.Parse("7d799596-3f6d-48e2-ac65-33ca6396788b");

        return new PodConnection(
            clientId,
            podModel,
            radioConnection,
            allocationLockDisposable,
            _configurationStore,
            _syncService);
    }
}