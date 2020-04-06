using System;
using System.Threading;
using System.Threading.Tasks;
using Nito.AsyncEx;
using OmniCore.Model.Interfaces.Common;
using OmniCore.Model.Interfaces.Services;
using OmniCore.Model.Interfaces.Services.Internal;

namespace OmniCore.Services
{
    public class CoreRepositoryService : CoreServiceBase, ICoreRepositoryService
    {
        private readonly AsyncReaderWriterLock ContextLock;
        private readonly ICoreApplicationFunctions CoreApplicationFunctions;
        private readonly ICoreLoggingFunctions Logging;
        private readonly ICoreContainer<IServerResolvable> ServerContainer;


        public CoreRepositoryService(ICoreContainer<IServerResolvable> serverContainer,
            ICoreApplicationFunctions coreApplicationFunctions,
            ICoreLoggingFunctions logging)
        {
            Logging = logging;
            ServerContainer = serverContainer;
            CoreApplicationFunctions = coreApplicationFunctions;
            ContextLock = new AsyncReaderWriterLock();
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

        public async Task<IRepositoryContextReadOnly> GetContextReadOnly(CancellationToken cancellationToken)
        {
            var readerLock = await ContextLock.ReaderLockAsync(cancellationToken);
            var context = ServerContainer.Get<IRepositoryContextReadOnly>();
            context.SetLock(readerLock, false);
            return context;
        }

        public async Task<IRepositoryContextReadWrite> GetContextReadWrite(CancellationToken cancellationToken)
        {
            var writerLock = await ContextLock.WriterLockAsync(cancellationToken);
            var context = ServerContainer.Get<IRepositoryContextReadWrite>();
            context.SetLock(writerLock, true);
            return context;
        }

        protected override async Task OnStart(CancellationToken cancellationToken)
        {
            Logging.Debug("Starting repository service");
            using var context = await GetContextReadWrite(cancellationToken);
// #if DEBUG
//             await context.InitializeDatabase(cancellationToken, true);
// #else
            await context.InitializeDatabase(cancellationToken, false);
//#endif
            Logging.Debug("Repository service started");
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
    }
}