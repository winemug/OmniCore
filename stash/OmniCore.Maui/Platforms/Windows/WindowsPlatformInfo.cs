using OmniCore.Services.Interfaces.Platform;

namespace OmniCore.Maui
{
    public class WindowsPlatformInfo : IPlatformInfo
    {
        public string SoftwareVersion => "";
        public string HardwareVersion => "";
        public string OsVersion => "";
        public string Platform => "";
        public Task<bool> VerifyPermissions()
        {
            return Task.FromResult<bool>(true);
        }

        public void OnRequestPermissionsResult(int requestCode, string[] permissions, int[] grantResults)
        {
        }
    }
}