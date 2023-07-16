using OmniCore.Common.Amqp;

namespace OmniCore.Common.Core;

public interface IAmqpService
{
    void PublishMessage(AmqpMessage message);
    void RegisterMessageHandler(Func<AmqpMessage, Task<bool>> handler);
}