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
    public class GenericRepository<T, U> : BasicRepository<T,U>, IGenericRepository<U> where U : IEntity where T : Entity, U, new()
    {

        public GenericRepository(IRepositoryService repositoryService) : base(repositoryService)
        {
        }

        public async Task Delete(U entity)
        {
            await Connection.DeleteAsync(entity);
        }

        public async Task Hide(U entity)
        {
            if (!entity.Hidden)
            {
                entity.Hidden = true;
                await Update(entity);
            }
        }

        public async Task Restore(U entity)
        {
            if (entity.Hidden)
            {
                entity.Hidden = false;
                await Update(entity);
            }
        }

        public async Task Update(U entity)
        {
            await Connection.UpdateAsync(entity);
        }
    }
}
