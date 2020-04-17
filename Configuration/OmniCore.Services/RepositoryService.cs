using System;
using System.Threading;
using System.Threading.Tasks;
using Nito.AsyncEx;
using OmniCore.Model.Interfaces.Common;
using OmniCore.Model.Interfaces.Services;
using OmniCore.Model.Interfaces.Services.Internal;

namespace OmniCore.Services
{
    public class RepositoryService : ServiceBase, IRepositoryService
    {
        private readonly AsyncReaderWriterLock ContextLock;
        private readonly ICommonFunctions CommonFunctions;
        private readonly ILogger Logger;
        private readonly IContainer<IServiceInstance> ServerContainer;


        public RepositoryService(IContainer<IServiceInstance> serverContainer,
            ICommonFunctions commonFunctions,
            ILogger logger)
        {
            Logger = logger;
            ServerContainer = serverContainer;
            CommonFunctions = commonFunctions;
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
            Logger.Debug("Starting repository service");
            using var context = await GetContextReadWrite(cancellationToken);
// #if DEBUG
//             await context.InitializeDatabase(cancellationToken, true);
// #else
            await context.InitializeDatabase(cancellationToken, false);
//#endif
            Logger.Debug("Repository service started");
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