using System.Threading.Tasks;
using OmniCore.Services.Interfaces.Amqp;

namespace OmniCore.Services.Interfaces.Core;

public interface IAmqpService : ICoreService
{
    void SetEndpoint(AmqpEndpoint endpoint);
    Task PublishMessage(AmqpMessage message);
}