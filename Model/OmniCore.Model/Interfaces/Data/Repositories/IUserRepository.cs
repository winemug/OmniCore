using System.Threading;
using System.Threading.Tasks;
using OmniCore.Model.Interfaces.Platform.Common.Data.Entities;
using SQLite;

namespace OmniCore.Model.Interfaces.Platform.Common.Data.Repositories
{
    public interface IUserRepository : IRepository<IUserEntity>
    {
        Task EnsureDefaults(SQLiteAsyncConnection connection, CancellationToken cancellationToken);
        Task<IUserEntity> GetDefaultUser(CancellationToken cancellationToken);
    }
}
