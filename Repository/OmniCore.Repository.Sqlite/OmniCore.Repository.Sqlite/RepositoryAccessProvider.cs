using System;
using System.Threading;
using System.Threading.Tasks;
using Nito.AsyncEx;
using Nito.AsyncEx.Synchronous;
using OmniCore.Model.Interfaces.Data.Repositories;
using OmniCore.Model.Interfaces.Platform.Common.Data.Repositories;
using SQLite;

namespace OmniCore.Repository.Sqlite
{
    public class RepositoryAccessProvider : IRepositoryAccessProvider
    {

        private readonly AsyncReaderWriterLock AccessLock;
        private SQLiteAsyncConnection Connection; 
        public RepositoryAccessProvider(string dataPath)
        {
            DataPath = dataPath;
            Connection = new SQLiteAsyncConnection(dataPath, SQLiteOpenFlags.Create | SQLiteOpenFlags.ReadWrite);
            AccessLock = new AsyncReaderWriterLock();
        }

        public string DataPath { get; }

        public async Task<IRepositoryAccess> ForData(CancellationToken cancellationToken)
        {
            using var repositoryLock = await AccessLock.ReaderLockAsync(cancellationToken);
            return new RepositoryAccess(Connection, repositoryLock);
        }
        
        public async Task<IRepositoryAccess> ForSchema(CancellationToken cancellationToken)
        {
            using var repositoryLock = await AccessLock.WriterLockAsync(cancellationToken);
            return new RepositoryAccess(Connection, repositoryLock);
        }

        public void Dispose()
        {
            using var _ = AccessLock.WriterLock();
            Connection?.CloseAsync().WaitWithoutException();
            Connection = null;
        }
    }
}