using Android.App;
using Android.Content.PM;
using Android.OS;
using OmniCore.Services.Interfaces.Core;
using OmniCore.Services.Interfaces.Platform;

namespace OmniCore.Maui;

[Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
public class MainActivity : MauiAppCompatActivity
{
}
