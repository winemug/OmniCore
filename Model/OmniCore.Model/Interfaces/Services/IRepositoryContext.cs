using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using OmniCore.Model.Interfaces.Entities;

namespace OmniCore.Model.Interfaces.Services
{
    public interface IRepositoryContext
    {
        IEntitySet<T> RegisterEntity<T>() where T : class, IEntity;
        Task RunMigrations(CancellationToken cancellationToken);
    }
}
