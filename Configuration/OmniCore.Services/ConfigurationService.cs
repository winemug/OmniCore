using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using OmniCore.Model.Entities;
using OmniCore.Model.Enumerations;
using OmniCore.Model.Exceptions;
using OmniCore.Model.Interfaces;
using OmniCore.Model.Interfaces.Services;
using OmniCore.Services.Configuration;

namespace OmniCore.Services
{
    public class ConfigurationService : IConfigurationService
    {
        private readonly IContainer Container;
        private readonly ILogger Logger;
        private readonly IRepositoryService RepositoryService;

        public ConfigurationService(IContainer container,
            IRepositoryService repositoryService,
            ILogger logger)
        {
            Logger = logger;
            Container = container;
            RepositoryService = repositoryService;
        }

        public Task<IDashConfiguration> GetDefaultDashConfiguration(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task SetDefaultDashConfiguration(IDashConfiguration dashConfiguration, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<IErosConfiguration> GetDefaultErosConfiguration(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task SetDefaultErosConfiguration(IErosConfiguration erosConfiguration, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public async Task<IMedication> GetDefaultMedication(CancellationToken cancellationToken)
        {
            try
            {
                using var context = await RepositoryService.GetContextReadOnly(cancellationToken);
                var entity = await context.Medications.FirstAsync(m => m.Hormone == HormoneType.Unknown);
                return new Medication {Entity = entity};
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception e)
            {
                throw new OmniCoreRepositoryException(FailureType.RepositoryGeneralError, "Failed to read default medication", e);
            }
        }

        public Task SetDefaultMedication(IMedication medication, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public async Task<IUser> GetDefaultUser(CancellationToken cancellationToken)
        {
            try
            {
                using var context = await RepositoryService.GetContextReadOnly(cancellationToken);
                var entity = await context.Users.FirstAsync(u => !u.ManagedRemotely);
                return new User {Entity = entity};
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception e)
            {
                throw new OmniCoreRepositoryException(FailureType.RepositoryGeneralError, "Failed to read default user", e);
            }
        }

        public Task SetDefaultUser(IUser user, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<BleOptions> GetBlePeripheralOptions(CancellationToken cancellationToken)
        {
            return Task.FromResult(new BleOptions());
        }

        public async Task SetBlePeripheralOptions(BleOptions bleOptions, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}