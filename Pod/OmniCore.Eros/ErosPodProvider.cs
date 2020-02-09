using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Nito.AsyncEx;
using OmniCore.Model.Entities;
using OmniCore.Model.Interfaces.Base;
using OmniCore.Model.Interfaces.Common;
using OmniCore.Model.Interfaces.Services.Facade;
using OmniCore.Model.Interfaces.Services.Internal;

namespace OmniCore.Eros
{
    public class ErosPodProvider : IErosPodProvider
    {
        private readonly ICoreContainer<IServerResolvable> Container;
        private readonly ConcurrentDictionary<long, IPodEros> PodDictionary;
        private readonly AsyncLock PodLock;

        public ErosPodProvider(ICoreContainer<IServerResolvable> container)
        {
            Container = container;
            PodDictionary = new ConcurrentDictionary<long, IPodEros>();
            PodLock = new AsyncLock();
        }
        public async Task<IList<IPodEros>> ActivePods(CancellationToken cancellationToken)
        {
            var context = Container.Get<IRepositoryContext>();
            var pods = new List<IPodEros>();
            context.Pods.Where(p => !p.IsDeleted)
                .Include(p => p.Medication)
                .Include(p => p.Radios)
                .Include(p => p.User)
                .ToList()
                .ForEach(async p => pods.Add(await GetPodInternal(p)));
            return pods;
        }
        
        private async Task<IPodEros> GetPodInternal(PodEntity podEntity)
        {
            using var podLock = await PodLock.LockAsync();
            if (PodDictionary.ContainsKey(podEntity.Id))
                return PodDictionary[podEntity.Id];

            var pod = Container.Get<ErosPod>();
            pod.Entity = podEntity;
            PodDictionary[podEntity.Id] = pod;
            return pod;
        }
    }
}