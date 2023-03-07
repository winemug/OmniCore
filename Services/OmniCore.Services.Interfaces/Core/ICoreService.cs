using System.Threading.Tasks;

namespace OmniCore.Services.Interfaces.Core;

public interface ICoreService
{
    Task Start();
    Task Stop();
}