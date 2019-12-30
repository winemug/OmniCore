using Nito.AsyncEx;
using OmniCore.Model.Interfaces.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using OmniCore.Model.Enumerations;
using OmniCore.Model.Exceptions;
using OmniCore.Model.Interfaces;
using OmniCore.Model.Interfaces.Data;
using OmniCore.Model.Interfaces.Data.Entities;
using OmniCore.Model.Interfaces.Data.Repositories;
using OmniCore.Services;
using SQLite;

namespace OmniCore.Repository.Sqlite
{
    public class RepositoryService : OmniCoreService, IRepositoryService
    {
        public string RepositoryPath { get; private set; }

        private readonly AsyncReaderWriterLock RepositoryAccessLock;
        private bool IsInitialized;
        private SQLiteAsyncConnection ConnectionInternal;

        private readonly ICoreContainer Container;

        public RepositoryService(ICoreContainer container) : base(null)
        {
            Container = container;
            RepositoryAccessLock = new AsyncReaderWriterLock();
        }

        public async Task<IRepositoryAccess> GetAccess(CancellationToken cancellationToken)
        {
            using var repositoryLock = await RepositoryAccessLock.ReaderLockAsync(cancellationToken);
            if (!IsInitialized)
                throw new OmniCoreWorkflowException(FailureType.WorkflowRepositoryNotInitialized);

            return new RepositoryAccess(ConnectionInternal, repositoryLock);
        }

        public Task Import(string importPath, CancellationToken cancellationToken)
        {
            if (!IsInitialized)
                throw new OmniCoreWorkflowException(FailureType.WorkflowRepositoryNotInitialized);

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

            ConnectionInternal = new SQLiteAsyncConnection
                (repositoryPath, SQLiteOpenFlags.Create | SQLiteOpenFlags.ReadWrite);

            var repositories = Container.AllAssignable<IRepository<IEntity>>();

            foreach (var repository in repositories)
            {
                if (repository != null)
                    await repository.Initialize(null, ConnectionInternal, cancellationToken);
            }

            IsInitialized = true;
        }

        public async Task Shutdown(CancellationToken cancellationToken)
        {
            using var repositoryLock = await RepositoryAccessLock.WriterLockAsync(cancellationToken);
            if (!IsInitialized)
                throw new OmniCoreWorkflowException(FailureType.WorkflowRepositoryNotInitialized);

            await ShutdownInternal(cancellationToken);
        }

        private async Task ShutdownInternal(CancellationToken cancellationToken)
        {
            if (ConnectionInternal != null)
                await ConnectionInternal?.CloseAsync();
            ConnectionInternal = null;
        }

        protected override Task OnStart(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        protected override Task OnStop(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        protected override Task OnPause(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        protected override Task OnResume(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
