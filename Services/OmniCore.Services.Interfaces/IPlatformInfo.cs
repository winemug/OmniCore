using System.Threading.Tasks;

namespace OmniCore.Services.Interfaces;

public interface IPlatformInfo
{
    string SoftwareVersion { get; }
    string HardwareVersion { get; }
    string Platform { get; }
    string OsVersion { get; }
    bool HasAllPermissions { get; }
    bool IsExemptFromBatteryOptimizations { get; }
    Task<bool> RequestMissingPermissions();
    void OpenBatteryOptimizationSettings();
}