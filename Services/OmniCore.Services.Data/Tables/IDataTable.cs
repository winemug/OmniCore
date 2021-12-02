using System.Threading.Tasks;
using Microsoft.Data.Sqlite;

namespace OmniCore.Services.Data.Tables
{
    public interface IDataTable
    {
       Task Create(SqliteConnection conn);
       Task ResetUpdates(SqliteConnection conn);
       Task CleanupDeleted(SqliteConnection conn);
    }
}