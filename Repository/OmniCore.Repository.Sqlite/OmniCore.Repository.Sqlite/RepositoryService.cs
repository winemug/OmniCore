using Nito.AsyncEx;
using OmniCore.Model.Interfaces.Platform;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
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

        private readonly ICoreServices Services;

        public RepositoryService(ICoreServices services) : base(null)
        {
            Services = services;
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

        private async Task ShutdownInternal(CancellationToken cancellationToken)
        {
            if (ConnectionInternal != null)
                await ConnectionInternal?.CloseAsync();
            ConnectionInternal = null;
        }

        protected override async Task OnStart(CancellationToken cancellationToken)
        {
            using var repositoryLock = await RepositoryAccessLock.WriterLockAsync(cancellationToken);
            if (IsInitialized)
                await ShutdownInternal(cancellationToken);

            var path = Path.Combine(Services.ApplicationService.DataPath, "oc.db3");

            var migrators = Services.Container.GetAll<IRepositoryMigrator>();

            foreach (var migrator in migrators)
            {
                await migrator.ExecuteMigration(
                    Services.ApplicationService.Version,
                    path, cancellationToken);
            }

            ConnectionInternal = new SQLiteAsyncConnection(path, SQLiteOpenFlags.ReadWrite);
            IsInitialized = true;
        }

        protected override async Task OnStop(CancellationToken cancellationToken)
        {
            using var repositoryLock = await RepositoryAccessLock.WriterLockAsync(cancellationToken);
            if (!IsInitialized)
                throw new OmniCoreWorkflowException(FailureType.WorkflowRepositoryNotInitialized);

            await ShutdownInternal(cancellationToken);
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
