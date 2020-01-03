using System;
using System.Collections.Generic;
using System.Text;
using OmniCore.Model.Interfaces.Data;
using OmniCore.Model.Interfaces.Data.Entities;
using OmniCore.Model.Interfaces.Data.Repositories;
using OmniCore.Model.Interfaces;
using OmniCore.Repository.Sqlite.Entities;

namespace OmniCore.Repository.Sqlite.Repositories
{
    public class RadioEventRepository : Repository<RadioEventEntity, IRadioEventEntity>, IRadioEventRepository
    {
        public RadioEventRepository(IRepositoryService repositoryService) : base(repositoryService)
        {
        }
    }
}
