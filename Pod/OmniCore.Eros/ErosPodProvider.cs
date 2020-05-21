using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using Nito.AsyncEx;
using OmniCore.Model.Entities;
using OmniCore.Model.Interfaces;
using OmniCore.Model.Interfaces.Services;
using OmniCore.Model.Interfaces.Services.Internal;
using AsyncLock = Nito.AsyncEx.AsyncLock;

namespace OmniCore.Eros
{
    public class ErosPodProvider : IErosPodProvider
    {
        private readonly IContainer Container;
        private readonly Dictionary<long, IErosPod> PodDictionary;
        private readonly IRepositoryService RepositoryService;
        private readonly AsyncLock PodDictionaryLock;

        public ErosPodProvider(
            IContainer container,
            IRepositoryService repositoryService)
        {
            RepositoryService = repositoryService;
            Container = container;
            PodDictionaryLock = new AsyncLock();
            PodDictionary = new Dictionary<long, IErosPod>();
        }

        public async Task<IList<IErosPod>> ActivePods(CancellationToken cancellationToken)
        {
            using var context = await RepositoryService.GetContextReadOnly(cancellationToken);
            var list = new List<IErosPod>();
            await context.Pods.Where(p => !p.IsDeleted)
                .Include(p => p.Medication)
                .Include(p => p.PodRadios)
                .ThenInclude(pr => pr.Radio)
                .Include(p => p.User)
                .ForEachAsync(async entity =>
                {
                    list.Add(await GetPodInternal(entity, cancellationToken));
                }, cancellationToken: cancellationToken);
            return list;
        }

        public async Task<IErosPod> NewPod(IUser user, IMedication medication, CancellationToken cancellationToken)
        {
            using var context = await RepositoryService.GetContextReadWrite(cancellationToken);
            context.WithExisting(medication.Entity, user.Entity);
            var entity = new PodEntity
            {
                Medication = medication.Entity,
                User = user.Entity,
                PodRadios = new List<PodRadioEntity>()
            };

            await context.Pods.AddAsync(entity, cancellationToken);
            await context.Save(cancellationToken);
            return await GetPodInternal(entity, cancellationToken);
        }

        private async Task<IErosPod> GetPodInternal(PodEntity podEntity, CancellationToken cancellationToken)
        {
            using (var _ = await PodDictionaryLock.LockAsync(cancellationToken))
            {
                if (!PodDictionary.ContainsKey(podEntity.Id))
                {
                    var pod = await Container.Get<ErosPod>();
                    await pod.Initialize(podEntity, cancellationToken);
                    PodDictionary[podEntity.Id] = pod;
                }
                return PodDictionary[podEntity.Id];
            }
        }

        public void Dispose()
        {
            foreach (var pod in PodDictionary.Values)
                pod.Dispose();
            
            PodDictionary.Clear();
        }
    }
}