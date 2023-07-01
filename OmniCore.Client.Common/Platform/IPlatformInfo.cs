namespace OmniCore.Common.Platform;

public interface IPlatformInfo
{
    //string SoftwareVersion { get; }
    //string HardwareVersion { get; }
    //string Platform { get; }
    //string OsVersion { get; }
    Task<bool> VerifyPermissions(bool silent);
    string? GetUserName();
    string GetVersion();
}