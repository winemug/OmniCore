namespace OmniCore.Services.Interfaces.Platform;

public interface IForegroundServiceHelper
{
    void StartForegroundService();
    void StopForegroundService();
}