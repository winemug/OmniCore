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
        public MedicationRepository(SQLiteAsyncConnection connection) : base(connection)
        {
        }

        public async Task<Medication> GetMedicationByName(string medName)
        {
            var c = await GetConnection();
            return await c.Table<Medication>().FirstOrDefaultAsync(x => x.Name == medName);
        }

        public async Task<List<Medication>> GetMedicationsByHormone(HormoneType hormone)
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
