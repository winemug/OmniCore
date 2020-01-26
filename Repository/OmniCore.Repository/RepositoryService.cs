using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using OmniCore.Model.Enumerations;
using OmniCore.Model.Interfaces.Data;
using OmniCore.Model.Interfaces.Platform.Common;
using OmniCore.Model.Interfaces.Platform.Server;
using OmniCore.Services;

namespace OmniCore.Repository
{
    public class RepositoryService : OmniCoreServiceBase, IRepositoryService
    {
        private readonly ICoreContainer<IServerResolvable> ServerContainer;
        private readonly ICoreApplicationFunctions CoreApplicationFunctions;
        public RepositoryService(ICoreContainer<IServerResolvable> serverContainer,
            ICoreApplicationFunctions coreApplicationFunctions)
        {
            ServerContainer = serverContainer;
            CoreApplicationFunctions = coreApplicationFunctions;
        }
        protected override async Task OnStart(CancellationToken cancellationToken)
        {
            await using var context = (RepositoryContext) ServerContainer.Get<IRepositoryContext>();
            await context.Database.MigrateAsync(cancellationToken);
            await context.SeedData();
        }

        protected override Task OnStop(CancellationToken cancellationToken)
        {
            //throw new NotImplementedException();
            return Task.CompletedTask;
        }

        protected override Task OnPause(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        protected override Task OnResume(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public IRepositoryContext GetContext()
        {
            return ServerContainer.Get<IRepositoryContext>();
        }

        public Task Import(string importPath, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task Restore(string backupPath, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task Backup(string backupPath, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
