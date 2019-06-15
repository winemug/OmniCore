using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using OmniCore.Mobile.Interfaces;

namespace OmniCore.Mobile.Android
{
        public class OmniCoreApplication : IOmniCoreApplication
    {
        public void Exit()
        {
            var activity = Application.Context as Activity;
            if (activity != null)
                activity.FinishAffinity();
            else
            {
                Process.KillProcess(Process.MyPid());
            }
        }
    }
}