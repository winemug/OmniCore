using OmniCore.Common.Amqp;

namespace OmniCore.Common.Core;

public interface IAmqpService : ICoreService
{
    Task PublishMessage(AmqpMessage message);
    void RegisterMessageProcessor(Func<AmqpMessage, Task<bool>> function);
}