using OmniCore.Model.Interfaces.Entities;
using OmniCore.Model.Interfaces.Repositories;
using OmniCore.Model.Interfaces.Services;
using OmniCore.Repository.Sqlite.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using SQLite;
using Unity;

namespace OmniCore.Repository.Sqlite.Repositories
{
    public class Repository<ConcreteType, InterfaceType> : IRepository<InterfaceType>
        where InterfaceType : IEntity
        where ConcreteType : Entity, InterfaceType, new()
    {
        private readonly IDataAccess DataAccess;
        private readonly IUnityContainer Container;

        public IExtendedAttributeProvider ExtendedAttributeProvider { get; set; }

        protected SQLiteAsyncConnection Connection
        {
            get { return DataAccess.Connection; }
        }

        public Repository(IDataAccess dataAccess,
            IUnityContainer container)
        {
            DataAccess = dataAccess;
            Container = container;
        }

        public InterfaceType New()
        {
            return new ConcreteType
            {
                ExtendedAttribute = ExtendedAttributeProvider?.New()
            };
        }
        public async Task Hide(InterfaceType entity)
        {
            if (!entity.Hidden)
            {
                entity.Hidden = true;
                await Update(entity);
            }
        }

        public async Task Restore(InterfaceType entity)
        {
            if (entity.Hidden)
            {
                entity.Hidden = false;
                await Update(entity);
            }
        }

        public async Task Delete(InterfaceType entity)
        {
            await Connection.DeleteAsync(entity);
        }

        public virtual async Task Update(InterfaceType entity)
        {
            await Connection.UpdateAsync(entity);
        }

        public virtual async Task Create(InterfaceType entity)
        {
            await Connection.InsertAsync(entity, typeof(ConcreteType));
        }
        public virtual async Task<InterfaceType> Read(long id)
        {
            var entity = await Connection.Table<ConcreteType>().FirstOrDefaultAsync(x => x.Id == id);
            if (entity != null)
                entity.ExtendedAttribute = ExtendedAttributeProvider?.New(entity.ExtensionValue);
            return entity;
        }

        public virtual async IAsyncEnumerable<InterfaceType> All()
        {
            var list = Connection.Table<ConcreteType>().Where(e => !e.Hidden);
            if (ExtendedAttributeProvider == null)
            {
                list = list.Where(e => e.ExtensionIdentifier == ExtendedAttributeProvider.Identifier);
            }
            foreach (var entity in await list.ToListAsync())
            {
                if (entity != null)
                    entity.ExtendedAttribute = ExtendedAttributeProvider?.New(entity.ExtensionValue);
                yield return entity;
            }
        }
    }
}
