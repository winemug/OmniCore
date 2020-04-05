using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using OmniCore.Model.Entities;

namespace OmniCore.Model.Interfaces.Services.Internal
{
    public interface IRepositoryContextReadWrite : IRepositoryContextReadOnly
    {
        Task Save(CancellationToken cancellationToken);
        IRepositoryContextReadWrite WithExisting(Entity entity);
    }
}
