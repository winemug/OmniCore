using System.Threading.Tasks;
using Microsoft.Data.Sqlite;

namespace OmniCore.Services.Interfaces;

public interface IDataService
{
    string DatabasePath { get; }
    void Start();
    void Stop();
    Task<SqliteConnection> GetConnectionAsync();
    Task InitializeDatabaseAsync();
    Task CopyDatabase(string destinationPath);
}