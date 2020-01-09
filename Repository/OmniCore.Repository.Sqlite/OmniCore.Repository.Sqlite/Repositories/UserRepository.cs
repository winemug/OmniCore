using OmniCore.Model.Interfaces.Common;
using OmniCore.Repository.Sqlite.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using OmniCore.Model.Interfaces.Common.Data;
using OmniCore.Model.Interfaces.Common.Data.Entities;
using OmniCore.Model.Interfaces.Common.Data.Repositories;

namespace OmniCore.Repository.Sqlite.Repositories
{
    public class UserRepository : Repository<UserEntity, IUserEntity>, IUserRepository
    {
        public UserRepository(IRepositoryService repositoryService) : base(repositoryService)
        {
        }
    }
}
