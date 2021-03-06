﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace OmniCore.Client.Droid.Platform
{
    public class BluetoothWakeLock : IDisposable
    {
        private string Tag = "OmniCoreLocationLock";
        private PowerManager.WakeLock WakeLockInternal;
        public BluetoothWakeLock()
        {
            PowerManager pm = (PowerManager)Application.Context.GetSystemService(Context.PowerService);
            Tag += Guid.NewGuid().ToString();
            WakeLockInternal = pm.NewWakeLock(WakeLockFlags.Partial | WakeLockFlags.LocationModeNoChange, Tag);
            WakeLockInternal.Acquire();
        }

        public void Dispose()
        {
            WakeLockInternal?.Release();
            WakeLockInternal = null;
        }
    }
}