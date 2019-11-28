using System;
using System.Collections.Generic;
using System.Text;
using OmniCore.Model.Interfaces.Entities;
using OmniCore.Model.Interfaces.Repositories;
using OmniCore.Model.Interfaces.Services;
using OmniCore.Repository.Sqlite.Entities;

namespace OmniCore.Repository.Sqlite.Repositories
{
    public class SignalStrengthRepository : BasicRepository<SignalStrengthEntity, ISignalStrengthEntity>, ISignalStrengthRepository
    {
        public SignalStrengthRepository(IRepositoryService repositoryService) : base(repositoryService)
        {
        }
    }
}
