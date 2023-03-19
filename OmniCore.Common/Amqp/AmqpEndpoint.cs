namespace OmniCore.Services.Interfaces.Amqp;

public class AmqpEndpoint
{
    public string UserId { get; set; }
    public string Dsn { get; set; }
    public string Queue { get; set; }
    public string Exchange { get; set; }
}