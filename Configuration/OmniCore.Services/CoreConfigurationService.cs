using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using OmniCore.Model.Enumerations;
using OmniCore.Model.Interfaces.Common;
using OmniCore.Model.Interfaces.Services;
using OmniCore.Model.Interfaces.Services.Facade;
using OmniCore.Model.Interfaces.Services.Internal;
using OmniCore.Services.Configuration;

namespace OmniCore.Services
{
    public class CoreConfigurationService : ICoreConfigurationService
    {
        private readonly ICoreContainer<IServerResolvable> Container;
        public CoreConfigurationService(ICoreContainer<IServerResolvable> container)
        {
            Container = container;
        }

        public Task<IDashConfiguration> GetDefaultDashConfiguration()
        {
            throw new NotImplementedException();
        }

        public Task SetDefaultDashConfiguration(IDashConfiguration dashConfiguration)
        {
            throw new NotImplementedException();
        }

        public async Task<IErosConfiguration> GetDefaultErosConfiguration()
        {
            throw new NotImplementedException();
        }

        public async Task SetDefaultErosConfiguration(IErosConfiguration erosConfiguration)
        {
            throw new NotImplementedException();
        }

        public async Task<IMedication> GetDefaultMedication()
        {
            var context = Container.Get<IRepositoryContext>();
            var entity = await context.Medications.FirstAsync(m => m.Hormone == HormoneType.Unknown);
            return new Medication() {Entity = entity};
        }

        public async Task SetDefaultMedication(IMedication medication)
        {
            throw new NotImplementedException();
        }

        public async Task<IUser> GetDefaultUser()
        {
            var context = Container.Get<IRepositoryContext>();
            var entity = await context.Users.FirstAsync(u => !u.ManagedRemotely);
            return new User() { Entity = entity};
        }

        public async Task SetDefaultUser(IUser user)
        {
            throw new NotImplementedException();
        }
    }
}
