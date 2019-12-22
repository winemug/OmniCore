using System;
using System.Collections.Generic;
using System.Text;
using OmniCore.Model.Interfaces.Data;
using OmniCore.Model.Interfaces.Data.Entities;
using OmniCore.Model.Interfaces.Data.Repositories;
using OmniCore.Model.Interfaces.Services;
using OmniCore.Repository.Sqlite.Entities;
using Unity;

namespace OmniCore.Repository.Sqlite.Repositories
{
    public class SignalStrengthRepository : Repository<SignalStrengthEntity, ISignalStrengthEntity>, ISignalStrengthRepository
    {
        public SignalStrengthRepository(IRepositoryService repositoryService, IUnityContainer container) : base(repositoryService, container)
        {
        }
    }
}
