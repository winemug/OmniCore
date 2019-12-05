using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Threading.Tasks;
using OmniCore.Model.Interfaces.Repositories;

namespace OmniCore.Model.Interfaces.Services
{
    public interface IRepositoryService
    {
        IDataAccess DataAccess { get; }
        bool IsInitialized { get; }
        string RepositoryPath { get; }
        Task Restore(string backupPath);
        Task Backup(string backupPath);
        Task<bool> IsValid(string repositoryPath);
        Task New(string repositoryPath);
        Task Initialize(string repositoryPath);
        Task Shutdown();

    }
}
