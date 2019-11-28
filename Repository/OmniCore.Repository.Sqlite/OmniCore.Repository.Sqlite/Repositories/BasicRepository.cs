using OmniCore.Model.Interfaces.Entities;
using OmniCore.Model.Interfaces.Repositories;
using OmniCore.Model.Interfaces.Services;
using OmniCore.Repository.Sqlite.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace OmniCore.Repository.Sqlite.Repositories
{
    public class BasicRepository<T, U> : Repository, IBasicRepository<U> where U : IBasicEntity where T : BasicEntity, U, new()
    {

        public BasicRepository(IRepositoryService repositoryService) : base(repositoryService)
        {
        }

        public async Task<U> New()
        {
            return default(T);
        }

        public async Task Create(U entity)
        {
            await Connection.InsertAsync(entity, typeof(T));
        }
        public async Task<U> Read(long id)
        {
            return await Connection.Table<T>().FirstOrDefaultAsync(x => x.Id == id);
        }

        public async IAsyncEnumerable<U> All()
        {
            var list = await Connection.Table<T>().ToListAsync();
            foreach (var entity in list)
            {
                yield return entity;
            }
        }
    }
}
