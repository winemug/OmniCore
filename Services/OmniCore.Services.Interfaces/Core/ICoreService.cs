using System.Threading.Tasks;

namespace OmniCore.Services.Interfaces;

public interface ICoreService
{
    Task Start();
    Task Stop();
}