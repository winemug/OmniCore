using OmniCore.Repository.Entities;
using OmniCore.Repository.Enums;
using SQLite;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace OmniCore.Repository
{
    public class MedicationRepository : SqliteRepositoryWithUpdate<Medication>
    {
        protected override async Task MigrateRepository(SQLiteAsyncConnection connection)
        {
            await base.MigrateRepository(connection);

            var rowCount = await connection.Table<Medication>().CountAsync();

            if (rowCount > 0)
                return;

            var meds = new []
            {
                new Medication { Hormone = HormoneType.Insulin, Name = "Novorapid U100", UnitName = "Units", UnitNameShort = "U", UnitsPerMilliliter = 100m, ProfileCode = "NRAP1" },
                new Medication { Hormone = HormoneType.Insulin, Name = "Fiasp U100", UnitName = "Units", UnitNameShort = "U", UnitsPerMilliliter = 100m, ProfileCode = "URAP1" }
            };

            foreach(var med in meds)
            {
                await this.Create(med);
            }
        }

        public async Task<Medication> GetMedicationByName(string medName)
        {
            var c = await GetConnection();
            return await c.Table<Medication>().FirstOrDefaultAsync(x => x.Name == medName);
        }

        public async Task<List<Medication>> GetMedicationByHormone(HormoneType hormone)
        {
            var c = await GetConnection();
            return await c.Table<Medication>().Where(m => m.Hormone == hormone).OrderBy(x => x.Name).ToListAsync();
        }

        public async Task<List<Medication>> GetMedications()
        {
            var c = await GetConnection();
            return await c.Table<Medication>().OrderBy(x => x.Name).ToListAsync();
        }
    }
}
