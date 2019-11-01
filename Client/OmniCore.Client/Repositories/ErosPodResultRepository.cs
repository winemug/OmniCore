using OmniCore.Eros;
using OmniCore.Model.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace OmniCore.Client.Repositories
{
    class ErosPodResultRepository : SqliteRepository<ErosResult>, IPodResultRepository<ErosResult>
    {
    }
}
