using Android.Content;
using Android.OS;
using OmniCore.Client.Abstractions.Services;
using OmniCore.Client.Platforms.Android;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmniCore.Client.Platforms;

public class PlatformForegroundService : IPlatformForegroundService
{
    public void StartForeground()
    {
        var activity = MauiApplication.Context;
        var intent = new Intent(activity, typeof(AndroidForegroundService));
        intent.SetAction(AndroidForegroundService.StartAction);

        if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
            activity.StartForegroundService(intent);
        else
            activity.StartService(intent);
    }

    public void StopForeground()
    {
        var activity = MauiApplication.Context;
        var intent = new Intent(activity, typeof(AndroidForegroundService));
        intent.SetAction(AndroidForegroundService.StopAction);
        activity.StartService(intent);
    }
}
