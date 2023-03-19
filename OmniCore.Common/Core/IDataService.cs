using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using OmniCore.Services.Interfaces.Pod;

namespace OmniCore.Services.Interfaces.Core;

public interface IDataService : ICoreService
{
    string DatabasePath { get; }
    Task<SqliteConnection> GetConnectionAsync();
    Task InitializeDatabaseAsync();
    Task CopyDatabase(string destinationPath);
    Task CreatePodMessage(Guid podId, int recordIndex, DateTimeOffset sendStart, DateTimeOffset receiveEnd, byte[] sentData, byte[] receivedData, ExchangeResult result);
}