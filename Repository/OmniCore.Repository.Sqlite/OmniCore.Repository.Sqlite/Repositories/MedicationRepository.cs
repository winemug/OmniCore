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

        public async Task EnsureDefaults(SQLiteAsyncConnection connection, CancellationToken cancellationToken)
        {
            DefaultMedication = await connection.Table<MedicationEntity>().FirstOrDefaultAsync(m => m.Hormone == HormoneType.Unknown);
            if (DefaultMedication == null)
            {
                var med = New();
                med.Hormone = HormoneType.Unknown;
                med.Name = "Unknown medication";
                med.UnitName = "millilitre";
                med.UnitsPerMilliliter = 1;
                med.UnitNameShort = "mL";

                await connection.InsertAsync(med);
                DefaultMedication = med;
            }
        }

        public async Task<IMedicationEntity> GetDefaultMedication(CancellationToken cancellationToken)
        {
            if (DefaultMedication == null)
            {
                using var access = await RepositoryService.GetAccess(cancellationToken);
                DefaultMedication = await access.Connection.Table<MedicationEntity>().FirstOrDefaultAsync(m => m.Hormone == HormoneType.Unknown);
            }
            return DefaultMedication;
        }
    }
}
