using System;
using System.Collections.Generic;
using System.Text;
using OmniCore.Model.Interfaces.Platform.Common.Data;
using OmniCore.Model.Interfaces.Platform.Common.Data.Entities;
using OmniCore.Model.Interfaces.Platform.Common.Data.Repositories;
using OmniCore.Model.Interfaces.Platform.Common;
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
