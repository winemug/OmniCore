using OmniCore.Model.Interfaces.Services;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Threading.Tasks;
using OmniCore.Model.Enumerations;
using OmniCore.Model.Exceptions;
using OmniCore.Model.Interfaces.Repositories;
using SQLite;

namespace OmniCore.Repository.Sqlite.Repositories
{
    public class DataAccess : IDataAccess
    {

        private readonly IRepositoryService RepositoryService;
        public DataAccess(IRepositoryService repositoryService)
        {
            RepositoryService = repositoryService;
        }

        private SQLiteAsyncConnection ConnectionInternal;
        public SQLiteAsyncConnection Connection
        {
            get
            {
                if (!RepositoryService.IsInitialized)
                    throw new OmniCoreRepositoryException(FailureType.LocalStorage, "Repository service is not initialized");

                if (ConnectionInternal == null)
                {
                    ConnectionInternal = new SQLiteAsyncConnection
                        (RepositoryService.RepositoryPath, SQLiteOpenFlags.Create | SQLiteOpenFlags.ReadWrite);

                }
                return ConnectionInternal;
            }
        }

        public void Dispose()
        {
        }
    }
}
