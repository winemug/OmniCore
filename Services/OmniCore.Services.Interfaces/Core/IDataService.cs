using System.Threading.Tasks;
using Microsoft.Data.Sqlite;

namespace OmniCore.Services.Interfaces.Core;

public interface IDataService : ICoreService
{
    string DatabasePath { get; }
    Task<SqliteConnection> GetConnectionAsync();
    Task InitializeDatabaseAsync();
    Task CopyDatabase(string destinationPath);
}