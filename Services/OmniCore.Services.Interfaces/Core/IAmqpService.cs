using System.Threading.Tasks;
using OmniCore.Services.Interfaces.Entities;

namespace OmniCore.Services.Interfaces;

public interface IAmqpService : ICoreService
{
    Task PublishMessage(AmqpMessage message);
}