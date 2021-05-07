using OmniCore.Model.Interfaces.Platform.Common;
using OmniCore.Repository.Sqlite.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using OmniCore.Model.Interfaces.Platform.Common.Data;
using OmniCore.Model.Interfaces.Platform.Common.Data.Entities;
using OmniCore.Model.Interfaces.Platform.Common.Data.Repositories;
using SQLite;

namespace OmniCore.Repository.Sqlite.Repositories
{
    public class UserRepository : Repository<UserEntity, IUserEntity>, IUserRepository
    {
        private IUserEntity DefaultUser;
        public UserRepository(IRepositoryService repositoryService) : base(repositoryService)
        {
        }

        public override async Task EnsureSchemaAndDefaults(CancellationToken cancellationToken)
        {
            await base.EnsureSchemaAndDefaults(cancellationToken);
            var defaultUser = await GetDefaultUser(cancellationToken);
          
            if (defaultUser == null)
            {
                defaultUser = New();
                defaultUser.ManagedRemotely = false;
                defaultUser.Name = "Default User";
                await Create(defaultUser, cancellationToken);
                DefaultUser = defaultUser;
            }
        }

        public async Task EnsureDefaults(SQLiteAsyncConnection connection, CancellationToken cancellationToken)
        {
        }

        public async Task<IUserEntity> GetDefaultUser(CancellationToken cancellationToken)
        {
            if (DefaultUser == null)
            {
                DefaultUser = await DataTask(c => c.Table<UserEntity>()
                    .OrderBy(u => u.Id).FirstOrDefaultAsync(), cancellationToken);
            }
            return DefaultUser;
        }
    }
}
