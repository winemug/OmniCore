using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
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

        public override IPodEntity New()
        {
            var entity = base.New();
            entity.ReservoirLowReminder = new ReminderAttributes();
            entity.ExpiresSoonReminder = new ReminderAttributes();
            entity.ExpiredReminder = new ReminderAttributes();
            entity.BasalScheduleEntries = new List<(TimeSpan,decimal)>();
            return entity;
        }

        public override async Task Create(IPodEntity entity, CancellationToken cancellationToken)
        {
            UpdateReferences((PodEntity)entity);
            await base.Create(entity, cancellationToken);
        }

        public override Task Update(IPodEntity entity, CancellationToken cancellationToken)
        {
            UpdateReferences((PodEntity)entity);
            return base.Update(entity, cancellationToken);
        }

        public override async Task<IPodEntity> Read(long id, CancellationToken cancellationToken)
        {
            var p = await base.Read(id, cancellationToken);
            await ReadReferences((PodEntity) p, cancellationToken);
            return p;
        }

        public async Task<IList<IPodEntity>> ActivePods(CancellationToken cancellationToken)
        {
            var list = await DataTask(c =>
                c.Table<PodEntity>().Where(e => !e.IsDeleted && e.State < PodState.Stopped)
                    .ToListAsync(), cancellationToken);

            foreach (var li in list)
                await ReadReferences(li, cancellationToken);
            return list.Select(l => (IPodEntity)l).ToList();
        }


        public async Task<IPodEntity> ByLotAndSerialNo(uint lot, uint serial, CancellationToken cancellationToken)
        {
            var p = await DataTask(c => c.Table<PodEntity>()
                .FirstOrDefaultAsync(p => p.Lot == lot
                                          && p.Serial == serial), cancellationToken);
            if (p != null)
                p = (PodEntity) await Read(p.Id, cancellationToken);

            return p;
        }

        public override async Task EnsureSchemaAndDefaults(CancellationToken cancellationToken)
        {
            await base.EnsureSchemaAndDefaults(cancellationToken);
#if DEBUG
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
                    p.Radio = radio;
                    p.User = await UserRepository.GetDefaultUser(CancellationToken.None);
                    p.Medication = await MedicationRepository.GetDefaultMedication(CancellationToken.None);
                    p.Lot = lot;
                    p.Serial = serial;
                    p.RadioAddress = radioAddress;
                    await Create(p, cancellationToken);
                }
            }, cancellationToken);
#endif
        }

        private void UpdateReferences(PodEntity ce)
        {
            ce.UserId = ce.User?.Id;
            ce.MedicationId = ce.Medication?.Id;
            ce.TherapyProfileId = ce.TherapyProfile?.Id;
            ce.ReferenceBasalScheduleId = ce.ReferenceBasalSchedule?.Id;
            ce.RadioId = ce.Radio?.Id;

            ce.ExpiresSoonReminderJson = JsonConvert.SerializeObject(ce.ExpiresSoonReminder);
            ce.ReservoirLowReminderJson = JsonConvert.SerializeObject(ce.ReservoirLowReminder);
            ce.ExpiredReminderJson = JsonConvert.SerializeObject(ce.ExpiredReminder);
            ce.BasalScheduleEntriesJson = JsonConvert.SerializeObject(ce.BasalScheduleEntries);
        }

        private async Task ReadReferences(PodEntity ce, CancellationToken cancellationToken)
        {
            if (ce.UserId.HasValue)
                ce.User = await UserRepository.Read(ce.UserId.Value, cancellationToken);

            if (ce.MedicationId.HasValue)
                ce.Medication = await MedicationRepository.Read(ce.MedicationId.Value, cancellationToken);

            //TODO: basal schedule ref and therapy profile

            ce.ExpiresSoonReminder = JsonConvert.DeserializeObject<ReminderAttributes>(ce.ExpiresSoonReminderJson);
            ce.ExpiredReminder = JsonConvert.DeserializeObject<ReminderAttributes>(ce.ExpiredReminderJson);
            ce.ReservoirLowReminder = JsonConvert.DeserializeObject<ReminderAttributes>(ce.ReservoirLowReminderJson);
            ce.BasalScheduleEntries = JsonConvert.DeserializeObject<List<(TimeSpan, decimal)>>(ce.BasalScheduleEntriesJson);
        }
    }
}