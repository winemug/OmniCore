using OmniCore.Model.Interfaces.Services;
using OmniCore.Repository.Sqlite.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using OmniCore.Model.Interfaces.Data;
using OmniCore.Model.Interfaces.Data.Entities;
using OmniCore.Model.Interfaces.Data.Repositories;

namespace OmniCore.Repository.Sqlite.Repositories
{
    public class UserRepository : Repository<UserEntity, IUserEntity>, IUserRepository
    {
        public UserRepository(IRepositoryService repositoryService) : base(repositoryService)
        {
        }
    }
}
