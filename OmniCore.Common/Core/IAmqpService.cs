using System.Threading.Tasks;
using OmniCore.Services.Interfaces.Amqp;

namespace OmniCore.Services.Interfaces.Core;

public interface IAmqpService : ICoreService
{
    public string Dsn { get; set; }
    public string Exchange { get; set; }
    public string Queue { get; set; }
    public string UserId { get; set; }
    Task PublishMessage(AmqpMessage message);
}