namespace OmniCore.Services.Interfaces.Amqp;

public record AmqpEndpoint
{
    public string Dsn { get; init; }
    public string Queue { get; init; }
    public string Exchange { get; init; }
}