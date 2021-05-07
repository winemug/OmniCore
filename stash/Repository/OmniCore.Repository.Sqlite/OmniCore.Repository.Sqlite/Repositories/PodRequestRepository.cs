using System.Threading;
using System.Threading.Tasks;
using OmniCore.Model.Interfaces.Platform.Common;
using OmniCore.Model.Interfaces.Platform.Common.Data.Entities;
using OmniCore.Model.Interfaces.Platform.Common.Data.Repositories;
using OmniCore.Repository.Sqlite.Entities;

namespace OmniCore.Repository.Sqlite.Repositories
{
    public class PodRequestRepository : Repository<PodRequestEntity, IPodRequestEntity>, IPodRequestRepository
    {
        private readonly IPodRepository PodRepository;
        public PodRequestRepository(IRepositoryService repositoryService,
            IPodRepository podRepository) : base(repositoryService)
        {
            PodRepository = podRepository;
        }

        public override Task Create(IPodRequestEntity entity, CancellationToken cancellationToken)
        {
            UpdateReferences((PodRequestEntity) entity);
            return base.Create(entity, cancellationToken);
        }

        public override Task Update(IPodRequestEntity entity, CancellationToken cancellationToken)
        {
            UpdateReferences((PodRequestEntity) entity);
            return base.Update(entity, cancellationToken);
        }

        public override async Task<IPodRequestEntity> Read(long id, CancellationToken cancellationToken)
        {
            var pr= await base.Read(id, cancellationToken);
            await ReadReferences((PodRequestEntity) pr, cancellationToken);
            return pr;
        }

        private void UpdateReferences(PodRequestEntity ce)
        {
            ce.PodId = ce.Pod?.Id;
        }

        private async Task ReadReferences(PodRequestEntity ce, CancellationToken cancellationToken)
        {
            if (ce.PodId.HasValue)
                ce.Pod = await PodRepository.Read(ce.PodId.Value, cancellationToken);
        }
    }
}