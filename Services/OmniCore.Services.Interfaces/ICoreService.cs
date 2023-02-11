using System.Threading.Tasks;

namespace OmniCore.Services.Interfaces
{
    public interface ICoreService
    {
        void Start();
        void Stop();
        Task PublishMessage(AmqpMessage message);
    }
}