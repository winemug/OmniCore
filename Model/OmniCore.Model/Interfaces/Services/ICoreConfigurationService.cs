using System.Threading;
using System.Threading.Tasks;
using OmniCore.Model.Interfaces.Common;
using OmniCore.Model.Interfaces.Services.Facade;

namespace OmniCore.Model.Interfaces.Services
{
    public interface ICoreConfigurationService : IServerResolvable
    {
        Task<IDashConfiguration> GetDefaultDashConfiguration(CancellationToken cancellationToken);
        Task SetDefaultDashConfiguration(IDashConfiguration dashConfiguration, CancellationToken cancellationToken);

        Task<IErosConfiguration> GetDefaultErosConfiguration(CancellationToken cancellationToken);
        Task SetDefaultErosConfiguration(IErosConfiguration erosConfiguration, CancellationToken cancellationToken);

        Task<IMedication> GetDefaultMedication(CancellationToken cancellationToken);
        Task SetDefaultMedication(IMedication medication, CancellationToken cancellationToken);

        Task<IUser> GetDefaultUser(CancellationToken cancellationToken);
        Task SetDefaultUser(IUser user, CancellationToken cancellationToken);
    }
}