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
        private readonly IContainer Container;
        private readonly ConcurrentDictionary<long, IErosPod> PodDictionary;
        private readonly IRepositoryService RepositoryService;

        public ErosPodProvider(
            IContainer container,
            IRepositoryService repositoryService)
        {
            RepositoryService = repositoryService;
            Container = container;
            PodDictionary = new ConcurrentDictionary<long, IErosPod>();
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
                    list.Add(await GetPodInternal(entity));
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
            return await GetPodInternal(entity);
        }

        private async Task<IErosPod> GetPodInternal(PodEntity podEntity)
        {
            var pod = await Container.Get<ErosPod>();
            return PodDictionary.GetOrAdd(podEntity.Id, id =>
            {
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