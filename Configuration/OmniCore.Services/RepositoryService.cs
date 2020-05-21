using System;
using System.Threading;
using System.Threading.Tasks;
using Nito.AsyncEx;
using OmniCore.Model.Interfaces;
using OmniCore.Model.Interfaces.Services;
using OmniCore.Model.Interfaces.Services.Internal;

namespace OmniCore.Services
{
    public class RepositoryService : ServiceBase, IRepositoryService
    {
        private readonly AsyncReaderWriterLock ContextLock;
        private readonly IPlatformFunctions PlatformFunctions;
        private readonly ILogger Logger;
        private readonly IContainer ServerContainer;


        public RepositoryService(IContainer serverContainer,
            IPlatformFunctions platformFunctions,
            ILogger logger)
        {
            Logger = logger;
            ServerContainer = serverContainer;
            PlatformFunctions = platformFunctions;
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
            var context = await ServerContainer.Get<IRepositoryContextReadOnly>();
            context.SetLock(readerLock, false);
            return context;
        }

        public async Task<IRepositoryContextReadWrite> GetContextReadWrite(CancellationToken cancellationToken)
        {
            var writerLock = await ContextLock.WriterLockAsync(cancellationToken);
            var context = await ServerContainer.Get<IRepositoryContextReadWrite>();
            context.SetLock(writerLock, true);
            return context;
        }

        protected override async Task OnStart(CancellationToken cancellationToken)
        {
            Logger.Debug("Starting repository service");
            using var context = await GetContextReadWrite(cancellationToken);
#if DEBUG
             await context.InitializeDatabase(cancellationToken, true);
#else
            await context.InitializeDatabase(cancellationToken, false);
#endif
            Logger.Debug("Repository service started");
        }

        protected override Task OnStop(CancellationToken cancellationToken)
        {
            //throw new NotImplementedException();
            return Task.CompletedTask;
        }
    }
}