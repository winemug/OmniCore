namespace OmniCore.Services.Interfaces;

public interface IForegroundServiceHelper
{
    ICoreService ForegroundService { get; set; }
    void StartForegroundService();
    void StopForegroundService();
}