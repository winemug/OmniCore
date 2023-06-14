using System.Threading.Tasks;
using OmniCore.Services.Interfaces.Amqp;

namespace OmniCore.Services.Interfaces.Core;

public interface IAmqpService : ICoreService
{
    Task PublishMessage(AmqpMessage message);
    void RegisterMessageProcessor(Func<AmqpMessage, Task<bool>> function);
}