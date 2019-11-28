using System;
using System.Collections.Generic;
using System.Text;
using OmniCore.Model.Interfaces.Entities;
using OmniCore.Model.Interfaces.Repositories;
using OmniCore.Model.Interfaces.Services;
using OmniCore.Repository.Sqlite.Entities;

namespace OmniCore.Repository.Sqlite.Repositories
{
    public class RadioEventRepository : BasicRepository<RadioEventEntity, IRadioEventEntity>, IRadioEventRepository
    {
        public RadioEventRepository(IRepositoryService repositoryService) : base(repositoryService)
        {
        }
    }
}
