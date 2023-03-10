namespace OmniCore.Services.Interfaces.Platform;

public interface IPlatformInfo
{
    string SoftwareVersion { get; }
    string HardwareVersion { get; }
    string Platform { get; }
    string OsVersion { get; }
    Task<bool> VerifyPermissions();
}