using System.Threading.Tasks;
using OmniCore.Services.Interfaces;

namespace OmniCore.Services.Interfaces;

public interface IAmqpService
{
    void Start();
    void Stop();
    Task PublishMessage(AmqpMessage message);
}