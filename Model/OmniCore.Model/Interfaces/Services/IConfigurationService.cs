using System.Threading.Tasks;
using OmniCore.Model.Interfaces.Platform.Common;
using OmniCore.Model.Interfaces.Services.ServiceEntities;
using IUser = OmniCore.Model.Interfaces.Services.ServiceEntities.IUser;

namespace OmniCore.Model.Interfaces.Services
{
    public interface IConfigurationService : IServerResolvable
    {
        Task<IDashConfiguration> GetDefaultDashConfiguration();
        Task SetDefaultDashConfiguration(IDashConfiguration dashConfiguration);

        Task<IErosConfiguration> GetDefaultErosConfiguration();
        Task SetDefaultErosConfiguration(IErosConfiguration erosConfiguration);

        Task<IMedication> GetDefaultMedication();
        Task SetDefaultMedication(IMedication medication);
        
        Task<IUser> GetDefaultUser();
        Task SetDefaultUser(IUser user);
    }
}