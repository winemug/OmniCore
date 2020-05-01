using System.Threading;
using System.Threading.Tasks;
using OmniCore.Model.Entities;

namespace OmniCore.Model.Interfaces.Services
{
    public interface IConfigurationService 
    {
        Task<IDashConfiguration> GetDefaultDashConfiguration(CancellationToken cancellationToken);
        Task SetDefaultDashConfiguration(IDashConfiguration dashConfiguration, CancellationToken cancellationToken);

        Task<IErosConfiguration> GetDefaultErosConfiguration(CancellationToken cancellationToken);
        Task SetDefaultErosConfiguration(IErosConfiguration erosConfiguration, CancellationToken cancellationToken);

        Task<IMedication> GetDefaultMedication(CancellationToken cancellationToken);
        Task SetDefaultMedication(IMedication medication, CancellationToken cancellationToken);

        Task<IUser> GetDefaultUser(CancellationToken cancellationToken);
        Task SetDefaultUser(IUser user, CancellationToken cancellationToken);

        Task<BleOptions> GetBlePeripheralOptions(CancellationToken cancellationToken);
        Task SetBlePeripheralOptions(BleOptions bleOptions, CancellationToken cancellationToken);
    }
}