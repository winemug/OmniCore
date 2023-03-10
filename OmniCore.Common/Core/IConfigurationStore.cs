using System.Threading.Tasks;
using OmniCore.Services.Interfaces.Configuration;

namespace OmniCore.Services.Interfaces.Core;

public interface IConfigurationStore
{
    Task<ClientConfiguration> GetConfigurationAsync();
    Task SetConfigurationAsync(ClientConfiguration cc);
}