namespace OmniCore.Common.Amqp;

public record AmqpMessageReceivedMessage
{
    public AmqpMessage Message { get; init; }
}