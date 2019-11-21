using OmniCore.Model.Interfaces.Services;
using SQLite;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace OmniCore.Repository.Sqlite.Repositories
{
    public class Repository
    {
        private string RepositoryPath;

        private SQLiteAsyncConnection ConnectionInternal;
        protected SQLiteAsyncConnection Connection
        {
            get
            {
                if (ConnectionInternal == null)
                {
                    ConnectionInternal = new SQLiteAsyncConnection(RepositoryPath, SQLiteOpenFlags.Create | SQLiteOpenFlags.ReadWrite);
                }
                return ConnectionInternal;
            }
        }
        public Repository(IRepositoryService repositoryService)
        {
            RepositoryPath = repositoryService.RepositoryPath;
        }
    }
}
