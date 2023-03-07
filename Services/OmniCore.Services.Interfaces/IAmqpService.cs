using System.Threading.Tasks;

namespace OmniCore.Services.Interfaces;

public interface IAmqpService
{
    void Start();
    void Stop();
    Task PublishMessage(AmqpMessage message);
}