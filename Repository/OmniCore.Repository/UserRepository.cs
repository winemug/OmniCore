using OmniCore.Repository.Entities;
using OmniCore.Repository.Enums;
using SQLite;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace OmniCore.Repository
{
    public class UserRepository : SqliteRepositoryWithUpdate<User>
    {
        public UserRepository(SQLiteAsyncConnection connection) : base(connection)
        {
        }

        public async Task<User> GetUserByName(string username)
        {
            var c = await GetConnection();
            return await c.Table<User>().FirstOrDefaultAsync(x => x.Name == username);
        }

        public async Task<List<User>> GetUsers(string username)
        {
            var c = await GetConnection();
            return await c.Table<User>().OrderBy(x => x.Name).ToListAsync();
        }
    }
}
