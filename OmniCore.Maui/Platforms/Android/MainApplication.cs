using Android.App;
using Android.Runtime;
using OmniCore.Services.Interfaces.Core;
using OmniCore.Services.Interfaces.Pod;
using OmniCore.Services.Interfaces.Radio;
using OmniCore.Services;
using OmniCore.Services.Interfaces.Platform;
using OmniCore.Maui.Services;

namespace OmniCore.Maui;

[Application(UsesCleartextTraffic = true)]
public class MainApplication : MauiApplication
{
    public MainApplication(IntPtr handle, JniHandleOwnership ownership)
        : base(handle, ownership)
    {
    }

    protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp(
        new PlatformInfo(), new PlatformService());
}