using System.Threading.Tasks;

namespace OmniCore.Services.Interfaces
{
    public interface IForegroundServiceHelper
    {
        IForegroundService Service { get; set; }
        void StartForegroundService();
        void StopForegroundService();

    }
}