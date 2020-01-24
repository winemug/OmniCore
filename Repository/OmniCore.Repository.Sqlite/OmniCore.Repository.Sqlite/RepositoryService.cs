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
using OmniCore.Model.Interfaces.Data.Repositories;
using OmniCore.Model.Interfaces.Platform.Common;
using OmniCore.Model.Interfaces.Platform.Common.Data;
using OmniCore.Model.Interfaces.Platform.Common.Data.Entities;
using OmniCore.Model.Interfaces.Platform.Common.Data.Repositories;
using OmniCore.Model.Interfaces.Services;
using OmniCore.Services;
using SQLite;

namespace OmniCore.Repository.Sqlite
{
    public class RepositoryService : OmniCoreServiceBase, IRepositoryService
    {
        private readonly ICoreContainer<IServerResolvable> Container;
        private readonly ICoreApplicationFunctions ApplicationFunctions;
        private readonly ICoreNotificationFunctions NotificationFunctions;

        private bool IsInitialized;
        public IRepositoryAccessProvider AccessProvider { get; private set; }
        
        public RepositoryService(ICoreContainer<IServerResolvable> container,
            ICoreApplicationFunctions applicationFunctions,
            ICoreNotificationFunctions notificationFunctions) : base(null)
        {
            Container = container;
            ApplicationFunctions = applicationFunctions;
            NotificationFunctions = notificationFunctions;
        }
        public Task Import(string importPath, CancellationToken cancellationToken)
        {
            if (!IsInitialized)
                throw new OmniCoreWorkflowException(FailureType.WorkflowRepositoryNotInitialized);

            throw new NotImplementedException();
        }

        public async Task Restore(string backupPath, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task Backup(string backupPath, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        private async Task ShutdownInternal(CancellationToken cancellationToken)
        {
            AccessProvider?.Dispose();
            AccessProvider = null;
        }

        protected override async Task OnStart(CancellationToken cancellationToken)
        {
            var path = Path.Combine(ApplicationFunctions.DataPath, "oc.db3");
            var accessProvider = new RepositoryAccessProvider(path);

            foreach (var migrator in Container.GetAll<IRepositoryMigrator>())
            {
                await migrator.ExecuteMigration(
                    ApplicationFunctions.Version,
                    accessProvider, cancellationToken);
            }
            AccessProvider = accessProvider;
        }

        protected override async Task OnStop(CancellationToken cancellationToken)
        {
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
