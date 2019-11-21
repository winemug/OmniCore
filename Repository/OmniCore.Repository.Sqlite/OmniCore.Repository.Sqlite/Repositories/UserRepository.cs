using OmniCore.Model.Interfaces.Entities;
using OmniCore.Model.Interfaces.Repositories;
using OmniCore.Model.Interfaces.Services;
using OmniCore.Repository.Sqlite.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace OmniCore.Repository.Sqlite.Repositories
{
    public class UserRepository : GenericRepository<UserEntity, IUserEntity>, IUserRepository
    {
        public UserRepository(IRepositoryService repositoryService) : base(repositoryService)
        {
        }
    }
}
