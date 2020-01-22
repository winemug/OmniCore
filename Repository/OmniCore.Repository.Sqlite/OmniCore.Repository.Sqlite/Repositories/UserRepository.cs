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

        public async Task EnsureDefaults(SQLiteAsyncConnection connection, CancellationToken cancellationToken)
        {
            DefaultUser = await connection.Table<UserEntity>().OrderBy(u => u.Id).FirstOrDefaultAsync();
            if (DefaultUser == null)
            {
                var user = New();
                user.ManagedRemotely = false;
                user.Name = "Default User";
                await connection.InsertAsync(user);
                DefaultUser = user;
            }
        }

        public async Task<IUserEntity> GetDefaultUser(CancellationToken cancellationToken)
        {
            if (DefaultUser == null)
            {
                using var access = await RepositoryService.GetAccess(cancellationToken);
                DefaultUser = await access.Connection.Table<UserEntity>().OrderBy(u => u.Id).FirstOrDefaultAsync();
            }
            return DefaultUser;
        }
    }
}
