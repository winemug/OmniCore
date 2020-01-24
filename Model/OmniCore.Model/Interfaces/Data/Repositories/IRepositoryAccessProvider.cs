using System;
using System.Threading;
using System.Threading.Tasks;
using OmniCore.Model.Interfaces.Platform.Common;
using OmniCore.Model.Interfaces.Platform.Common.Data.Repositories;

namespace OmniCore.Model.Interfaces.Data.Repositories
{
    public interface IRepositoryAccessProvider : IServerResolvable, IDisposable
    {
        string DataPath { get; }
        Task<IRepositoryAccess> ForData(CancellationToken cancellationToken);
        Task<IRepositoryAccess> ForSchema(CancellationToken cancellationToken);
    }
}