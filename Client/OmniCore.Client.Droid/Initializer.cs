using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using OmniCore.Client.Interfaces;
using OmniCore.Mobile.Droid;
using OmniCore.Mobile.Droid.Platform;
using OmniCore.Model.Interfaces;
using OmniCore.Model.Interfaces.Platform;
using Unity;

namespace OmniCore.Client.Droid
{
    public static class Initializer
    {
        public static IUnityContainer OnAndroid(this IUnityContainer container)
        {
            return container;
        }
    }
}