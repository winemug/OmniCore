using System;
using System.Threading;
using System.Threading.Tasks;
using OmniCore.Model.Interfaces.Common;
using OmniCore.Model.Interfaces.Services;
using OmniCore.Model.Interfaces.Services.Internal;

namespace OmniCore.Services
{
    public class CoreRepositoryService : OmniCoreServiceBase, ICoreRepositoryService
    {
        private readonly ICoreContainer<IServerResolvable> ServerContainer;
        private readonly ICoreApplicationFunctions CoreApplicationFunctions;
        public CoreRepositoryService(ICoreContainer<IServerResolvable> serverContainer,
            ICoreApplicationFunctions coreApplicationFunctions)
        {
            ServerContainer = serverContainer;
            CoreApplicationFunctions = coreApplicationFunctions;
        }
        protected override async Task OnStart(CancellationToken cancellationToken)
        {
            var context = ServerContainer.Get<IRepositoryContext>();
            #if DEBUG
            await context.InitializeDatabase(cancellationToken, true);
            #else
            await context.InitializeDatabase(cancellationToken, false);
            #endif
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
