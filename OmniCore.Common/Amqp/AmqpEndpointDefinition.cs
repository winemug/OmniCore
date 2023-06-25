namespace OmniCore.Services.Interfaces.Amqp;

public record AmqpEndpointDefinition
{
    public string UserId { get; init; }
    public string Dsn { get; init; }
    public string Queue { get; init; }
    public string Exchange { get; init; }
}