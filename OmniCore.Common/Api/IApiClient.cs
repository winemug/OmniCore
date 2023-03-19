using OmniCore.Services.Interfaces.Amqp;

namespace OmniCore.Common.Api;

public interface IApiClient : IDisposable
{
    Task AuthorizeAccountAsync(string email, string password);
    Task RegisterClientAsync();
    Task<AmqpEndpoint> GetClientEndpointAsync();
}