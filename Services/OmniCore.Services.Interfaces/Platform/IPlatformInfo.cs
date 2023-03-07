using System.Threading.Tasks;
using Android.Content.PM;

namespace OmniCore.Services.Interfaces.Platform;

public interface IPlatformInfo
{
    string SoftwareVersion { get; }
    string HardwareVersion { get; }
    string Platform { get; }
    string OsVersion { get; }
    bool HasAllPermissions { get; }
    bool IsExemptFromBatteryOptimizations { get; }
    Task<bool> RequestMissingPermissions();
    void OnRequestPermissionsResult(int requestCode, string[] permissions, Permission[] grantResults);
    void OpenBatteryOptimizationSettings();
}