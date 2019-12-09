using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SQLite;

namespace OmniCore.Model.Interfaces.Repositories
{
    public interface IRepositoryInitialization
    {
        Task Initialize(Version? migrateFrom, SQLiteAsyncConnection connection, CancellationToken cancellationToken);
    }
}
