using Android.Content;
using Android.OS;
using OmniCore.Maui.Interfaces;
using static Microsoft.Maui.ApplicationModel.Permissions;
using Debug = System.Diagnostics.Debug;

namespace OmniCore.Maui;
public class PlatformInfo : IPlatformInfo
{
    //public partial string SoftwareVersion { get; }
    //public partial string HardwareVersion { get; }
    //public partial string OsVersion { get; }
    //public partial string Platform { get; }

    public string GetUserName()
    {
        //TODO:
        return "";
    }

    public string GetVersion()
    {
        return "1.0.0.0";
    }

    //public PlatformInfo()
    //{
    //    Platform = "Android";
    //    HardwareVersion = "1.0.0";
    //    SoftwareVersion = "1.0.0";
    //    OsVersion = "10";
    //}
}