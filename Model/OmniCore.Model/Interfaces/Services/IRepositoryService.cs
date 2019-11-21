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
        Task InitializeNew(string repositoryPath);
        Task Initialize(string repositoryPath);
    }
}
