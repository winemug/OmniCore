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
using OmniCore.Model.Interfaces.Common;
using OmniCore.Model.Interfaces.Services;
using OmniCore.Model.Interfaces.Services.Facade;
using OmniCore.Model.Interfaces.Services.Internal;

namespace OmniCore.Eros
{
    public class ErosPodProvider : IErosPodProvider
    {
        private readonly ICoreContainer<IServerResolvable> Container;
        private readonly ConcurrentDictionary<long, IErosPod> PodDictionary;
        private readonly ICoreRepositoryService RepositoryService;

        public ErosPodProvider(
            ICoreContainer<IServerResolvable> container,
            ICoreRepositoryService repositoryService)
        {
            RepositoryService = repositoryService;
            Container = container;
            PodDictionary = new ConcurrentDictionary<long, IErosPod>();
        }

        public async Task<IList<IErosPod>> ActivePods(CancellationToken cancellationToken)
        {
            using var context = await RepositoryService.GetContextReadOnly(cancellationToken);
            return context.Pods.Where(p => !p.IsDeleted)
                .Include(p => p.Medication)
                .Include(p => p.PodRadios)
                .ThenInclude(pr => pr.Radio)
                .Include(p => p.User)
                .ToList()
                .Select(GetPodInternal)
                .ToList();
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
            return GetPodInternal(entity);
        }

        private IErosPod GetPodInternal(PodEntity podEntity)
        {
            return PodDictionary.GetOrAdd(podEntity.Id, id =>
            {
                var pod = Container.Get<ErosPod>();
                pod.Entity = podEntity;
                return pod;
            });
        }

        public void Dispose()
        {
            foreach (var pod in PodDictionary.Values)
                pod.Dispose();
            
            PodDictionary.Clear();
        }
    }
}