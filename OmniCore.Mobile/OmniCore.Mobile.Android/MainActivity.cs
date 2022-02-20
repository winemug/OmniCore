using System;

using Android.App;
using Android.Appwidget;
using Android.Content;
using Android.Content.PM;
using Android.Runtime;
using Android.OS;
using OmniCore.Services.Interfaces;
using Unity;
using Unity.Injection;


namespace OmniCore.Mobile.Droid
{
    [Activity(Label = "OmniCore.Mobile", Icon = "@mipmap/icon", Theme = "@style/MainTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize )]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
    {
        private PlatformInfo _platformInfo;
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            //App.Container.RegisterType<IPlatformInfo, PlatformInfo>(new InjectionConstructor(this));
            _platformInfo = new PlatformInfo(this, this); 
            App.Container.RegisterInstance<IPlatformInfo>(_platformInfo);
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            global::Xamarin.Forms.Forms.Init(this, savedInstanceState);
            LoadApplication(new App());
        }


        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);
            _platformInfo.OnRequestPermissionsResult(requestCode, permissions, grantResults);
            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }
    }
}