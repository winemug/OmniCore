using OmniCore.Common.Amqp;

namespace OmniCore.Common.Core;

public interface IAmqpService
{
    Task PublishMessage(AmqpMessage message);
}