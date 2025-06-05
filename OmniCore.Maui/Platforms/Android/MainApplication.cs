using Android.App;
using Android.Runtime;

namespace OmniCore.Maui;

#if DEBUG
[Application(Debuggable = true, NetworkSecurityConfig = "@xml/network_security_config")]
#else
    [Application(Debuggable = false)]
#endif
public class MainApplication : MauiApplication
{
    public MainApplication(IntPtr handle, JniHandleOwnership ownership)
        : base(handle, ownership)
    {
    }

    protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();
}