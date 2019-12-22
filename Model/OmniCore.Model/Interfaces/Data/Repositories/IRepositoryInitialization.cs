using System;
using System.Threading;
using System.Threading.Tasks;
using SQLite;

namespace OmniCore.Model.Interfaces.Data.Repositories
{
    public interface IRepositoryInitialization
    {
        Task Initialize(Version migrateFrom, SQLiteAsyncConnection connection, CancellationToken cancellationToken);
    }
}
