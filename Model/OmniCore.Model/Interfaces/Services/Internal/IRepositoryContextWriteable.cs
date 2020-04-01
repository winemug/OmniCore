using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OmniCore.Model.Interfaces.Services.Internal
{
    public interface IRepositoryContextWriteable : IRepositoryContext
    {
        Task Save(CancellationToken cancellationToken);
    }
}
