using Nito.AsyncEx;
using OmniCore.Model.Interfaces.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using OmniCore.Model.Enumerations;
using OmniCore.Model.Exceptions;
using OmniCore.Model.Interfaces.Entities;
using OmniCore.Model.Interfaces.Repositories;
using Unity;
using Unity.Resolution;

namespace OmniCore.Repository
{
    public class RepositoryService : IRepositoryService
    {
        public string RepositoryPath { get; private set; }

        private readonly AsyncReaderWriterLock RepositoryAccessLock;
        private readonly IUnityContainer Container;
        private bool IsInitialized;
        private IRepositoryContext RepositoryContext;

        public RepositoryService(IUnityContainer container)
        {
            Container = container;
            RepositoryAccessLock = new AsyncReaderWriterLock();
        }

        public async Task<IRepositoryContext> GetContext(CancellationToken cancellationToken)
        {
            using var repositoryLock = await RepositoryAccessLock.ReaderLockAsync(cancellationToken);
            if (!IsInitialized)
                throw new OmniCoreRepositoryException(FailureType.LocalStorage, "Not initialized");

            return RepositoryContext;
        }

        public Task Import(string importPath, CancellationToken cancellationToken)
        {
            if (!IsInitialized)
                throw new OmniCoreRepositoryException(FailureType.LocalStorage, "Not initialized");

            throw new NotImplementedException();
        }


        public async Task Restore(string backupPath, CancellationToken cancellationToken)
        {
            using var repositoryLock = await RepositoryAccessLock.WriterLockAsync(cancellationToken);
            if (IsInitialized)
                await ShutdownInternal(cancellationToken);

            throw new NotImplementedException();
        }

        public Task Backup(string backupPath, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public async Task Initialize(string repositoryPath, CancellationToken cancellationToken)
        {
            using var repositoryLock = await RepositoryAccessLock.WriterLockAsync(cancellationToken);
            if (IsInitialized)
                await ShutdownInternal(cancellationToken);

            RepositoryPath = repositoryPath;

            RepositoryContext = new RepositoryContext(repositoryPath);
            await RepositoryContext.RunMigrations(cancellationToken);

            //var repositories = Container
            //    .Registrations
            //    .Where(r => r.RegisteredType.GetInterfaces().Any(i => i == typeof(IRepositoryInitialization)))
            //    .Select(x => Container.Resolve(x.RegisteredType, x.Name) as IRepositoryInitialization);

            //foreach (var repository in repositories)
            //{
            //    if (repository != null)
            //        await repository.Initialize(null, ConnectionInternal, cancellationToken);
            //}

            IsInitialized = true;
        }

        public async Task Shutdown(CancellationToken cancellationToken)
        {
            using var repositoryLock = await RepositoryAccessLock.WriterLockAsync(cancellationToken);
            if (!IsInitialized)
                throw new OmniCoreRepositoryException(FailureType.LocalStorage, "Not initialized");

            await ShutdownInternal(cancellationToken);
        }

        private async Task ShutdownInternal(CancellationToken cancellationToken)
        {
            //if (ConnectionInternal != null)
            //    await ConnectionInternal?.CloseAsync();
            //ConnectionInternal = null;
        }
    }
}
