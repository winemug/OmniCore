using System.Threading.Tasks;
using OmniCore.Services.Interfaces.Entities;

namespace OmniCore.Services.Interfaces;

public interface IConfigurationService: ICoreService
{
    Task<ClientConfiguration> GetConfigurationAsync();
    Task SetConfigurationAsync(ClientConfiguration cc);
}