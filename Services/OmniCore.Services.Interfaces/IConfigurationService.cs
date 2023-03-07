using System.Threading.Tasks;
using OmniCore.Services.Entities;

namespace OmniCore.Services.Interfaces;

public interface IConfigurationService
{ Task<ClientConfiguration> GetConfigurationAsync();
    Task SetConfigurationAsync(ClientConfiguration cc);
}