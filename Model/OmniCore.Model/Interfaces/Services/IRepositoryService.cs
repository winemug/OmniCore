using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace OmniCore.Model.Interfaces.Services
{
    public interface IRepositoryService
    {
        bool IsInitialized { get; }
        string RepositoryPath { get; }
        Task Restore(string backupPath);
        Task Backup(string backupPath);
        Task<bool> IsValid(string repositoryPath);
        Task New(string repositoryPath);
        Task Initialize(string repositoryPath);
    }
}
