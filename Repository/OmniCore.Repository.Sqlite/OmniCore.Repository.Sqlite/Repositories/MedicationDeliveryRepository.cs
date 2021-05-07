using System.Threading;
using System.Threading.Tasks;
using OmniCore.Model.Interfaces.Data.Repositories;
using OmniCore.Model.Interfaces.Platform.Common;
using OmniCore.Model.Interfaces.Platform.Common.Data.Entities;
using OmniCore.Model.Interfaces.Platform.Common.Data.Repositories;
using OmniCore.Repository.Sqlite.Entities;
using Unity;

namespace OmniCore.Repository.Sqlite.Repositories
{
    public class MedicationDeliveryRepository : Repository<MedicationDeliveryEntity, IMedicationDeliveryEntity>, IMedicationDeliveryRepository
    {
        private readonly IPodRepository PodRepository;
        private readonly IMedicationRepository MedicationRepository;
        private readonly IUserRepository UserRepository;

        public MedicationDeliveryRepository(IRepositoryService repositoryService,
            IMedicationRepository medicationRepository,
            IPodRepository podRepository,
            IUserRepository userRepository) : base(repositoryService)
        {
        }

        public override Task Create(IMedicationDeliveryEntity entity, CancellationToken cancellationToken)
        {
            UpdateReferences((MedicationDeliveryEntity)entity);
            return base.Create(entity, cancellationToken);
        }

        public override Task Update(IMedicationDeliveryEntity entity, CancellationToken cancellationToken)
        {
            UpdateReferences((MedicationDeliveryEntity)entity);
            return base.Update(entity, cancellationToken);
        }

        public override async Task<IMedicationDeliveryEntity> Read(long id, CancellationToken cancellationToken)
        {
            var ce = (MedicationDeliveryEntity) await base.Read(id, cancellationToken);
            await ReadReferences(ce, cancellationToken);
            return ce;
        }

        private void UpdateReferences(MedicationDeliveryEntity ce)
        {
            ce.MedicationId = ce.Medication?.Id;
            ce.PodId = ce.Pod?.Id;
            ce.UserId = ce.User?.Id;
        }

        private async Task ReadReferences(MedicationDeliveryEntity ce, CancellationToken cancellationToken)
        {
            if (ce.PodId.HasValue)
                ce.Pod = await PodRepository.Read(ce.PodId.Value, cancellationToken);

            if (ce.MedicationId.HasValue)
                ce.Medication = await MedicationRepository.Read(ce.MedicationId.Value, cancellationToken);

            if (ce.UserId.HasValue)
                ce.User = await UserRepository.Read(ce.UserId.Value, cancellationToken);
        }

    }
}