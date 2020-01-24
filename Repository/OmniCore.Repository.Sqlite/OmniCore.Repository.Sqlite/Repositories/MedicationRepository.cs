using OmniCore.Model.Interfaces.Platform.Common;
using OmniCore.Repository.Sqlite.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using OmniCore.Model.Enumerations;
using OmniCore.Model.Interfaces.Platform.Common.Data;
using OmniCore.Model.Interfaces.Platform.Common.Data.Entities;
using OmniCore.Model.Interfaces.Platform.Common.Data.Repositories;
using SQLite;

namespace OmniCore.Repository.Sqlite.Repositories
{
    public class MedicationRepository : Repository<MedicationEntity, IMedicationEntity>, IMedicationRepository
    {
        private IMedicationEntity DefaultMedication;
        public MedicationRepository(IRepositoryService repositoryService) : base(repositoryService)
        {
        }

        public override async Task EnsureSchemaAndDefaults(CancellationToken cancellationToken)
        {
            await base.EnsureSchemaAndDefaults(cancellationToken);

            await DataTask(async c =>
            {
                DefaultMedication = await c.Table<MedicationEntity>().FirstOrDefaultAsync(m => m.Hormone == HormoneType.Unknown);
                if (DefaultMedication == null)
                {
                    var med = New();
                    med.Hormone = HormoneType.Unknown;
                    med.Name = "Unknown medication";
                    med.UnitName = "millilitre";
                    med.UnitsPerMilliliter = 1;
                    med.UnitNameShort = "mL";

                    await Create(med, cancellationToken);
                    DefaultMedication = med;
                }
            }, cancellationToken);
        }

        public async Task<IMedicationEntity> GetDefaultMedication(CancellationToken cancellationToken)
        {
            if (DefaultMedication == null)
            {
                DefaultMedication = await DataTask(c => c.Table<MedicationEntity>()
                    .FirstOrDefaultAsync(m => m.Hormone == HormoneType.Unknown)
                    , cancellationToken);
            }
            return DefaultMedication;
        }
    }
}
