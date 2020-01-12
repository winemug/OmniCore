using System;
using OmniCore.Model.Interfaces.Platform.Common.Data.Repositories;
using OmniCore.Model.Interfaces.Platform.Common;
using SQLite;

namespace OmniCore.Repository.Sqlite
{
    public class RepositoryAccess : IRepositoryAccess
    {

        private IDisposable RepositoryLock;
        public RepositoryAccess(SQLiteAsyncConnection connection, IDisposable repositoryLock)
        {
            Connection = connection;
            RepositoryLock = repositoryLock;
        }

        public SQLiteAsyncConnection Connection { get; private set; }

        public void Dispose()
        {
            RepositoryLock?.Dispose();
            RepositoryLock = null;
            Connection = null;
        }
    }
}
