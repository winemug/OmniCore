using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using OmniCore.Model.Enumerations;
using OmniCore.Model.Interfaces.Platform.Common.Data;
using OmniCore.Model.Interfaces.Platform.Common.Data.Entities;
using OmniCore.Model.Interfaces.Platform.Common.Data.Repositories;
using OmniCore.Model.Interfaces.Platform.Common;
using OmniCore.Repository.Sqlite.Entities;
using SQLite;

namespace OmniCore.Repository.Sqlite.Repositories
{
    public class PodRepository : Repository<PodEntity, IPodEntity>, IPodRepository
    {
        private readonly IRadioRepository RadioRepository;
        private readonly IUserRepository UserRepository;
        private readonly IMedicationRepository MedicationRepository;
        public PodRepository(IRepositoryService repositoryService,
            IRadioRepository radioRepository,
            IUserRepository userRepository,
            IMedicationRepository medicationRepository) : base(repositoryService)
        {
            RadioRepository = radioRepository;
            UserRepository = userRepository;
            MedicationRepository = medicationRepository;
        }

        public async Task<IList<IPodEntity>> ActivePods(CancellationToken cancellationToken)
        {
            var list = await DataTask(c =>
                c.Table<PodEntity>().Where(e => !e.IsDeleted && e.State < PodState.Stopped)
                    .ToListAsync(), cancellationToken);
            return list.Select(l => (IPodEntity)l).ToList();
        }
        //
        // public async Task<IList<IPodEntity>> ArchivedPods(CancellationToken cancellationToken)
        // {
        //     return (IList<IPodEntity>) await DataTask(c =>
        //         c.Table<PodEntity>().Where(e => !e.IsDeleted && e.State < PodState.Stopped)
        //             .ToListAsync(), cancellationToken);
        // }

        public async Task<IPodEntity> ByLotAndSerialNo(uint lot, uint serial, CancellationToken cancellationToken)
        {
            return await DataTask(c => c.Table<PodEntity>()
                .FirstOrDefaultAsync(p => p.Lot == lot
                                          && p.Serial == serial), cancellationToken);
        }
#if DEBUG
        public override async Task EnsureSchemaAndDefaults(CancellationToken cancellationToken)
        {
            await base.EnsureSchemaAndDefaults(cancellationToken);
            uint lot = 45048;
            uint serial = 380019;
            uint radioAddress = 0x1f0e89f2;
            var radioId = Guid.Parse("00000000-0000-0000-0000-000780393d00");

            await DataTask(async (c) =>
            {
                RadioRepository.WithDirectAccess(DirectAccess);
                var radio = await RadioRepository.ByDeviceUuid(radioId, cancellationToken);

                var p = await ByLotAndSerialNo(lot, serial, cancellationToken);
                if (p == null)
                {
                    UserRepository.WithDirectAccess(DirectAccess);
                    MedicationRepository.WithDirectAccess(DirectAccess);
                    p = New();
                    p.Radios = new List<IRadioEntity> {radio};
                    p.User = await UserRepository.GetDefaultUser(CancellationToken.None);
                    p.Medication = await MedicationRepository.GetDefaultMedication(CancellationToken.None);
                    p.Lot = lot;
                    p.Serial = serial;
                    p.RadioAddress = radioAddress;
                    await Create(p, cancellationToken);
                }
            }, cancellationToken);
        }
#endif
    }
}