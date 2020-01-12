using Nito.AsyncEx;
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
using OmniCore.Model.Interfaces.Platform.Common;
using OmniCore.Model.Interfaces.Platform.Common.Data;
using OmniCore.Model.Interfaces.Platform.Common.Data.Entities;
using OmniCore.Model.Interfaces.Platform.Common.Data.Repositories;
using OmniCore.Services;
using SQLite;

namespace OmniCore.Repository.Sqlite
{
    public class RepositoryServiceBase : OmniCoreServiceBase, IRepositoryService
    {
        public string RepositoryPath { get; private set; }

        private readonly AsyncReaderWriterLock RepositoryAccessLock;
        private bool IsInitialized;
        private SQLiteAsyncConnection ConnectionInternal;

        private readonly ICoreContainer<IServerResolvable> Container;
        private readonly ICoreApplicationFunctions ApplicationFunctions;

        public RepositoryServiceBase(ICoreContainer<IServerResolvable> container,
            ICoreApplicationFunctions applicationFunctions) : base(null)
        {
            Container = container;
            ApplicationFunctions = applicationFunctions;
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

            var path = Path.Combine(ApplicationFunctions.DataPath, "oc.db3");

            var migrators = Container.GetAll<IRepositoryMigrator>();

            foreach (var migrator in migrators)
            {
                await migrator.ExecuteMigration(
                    ApplicationFunctions.Version,
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
