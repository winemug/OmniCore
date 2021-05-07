﻿using System;
using Android.App;
using Android.Content;
using Android.OS;

namespace OmniCore.Client.Droid
{
    public class DisplayLock : IDisposable
    {
        private readonly string Tag = "OmniCoreScreenLock";
        private PowerManager.WakeLock WakeLockInternal;

        public DisplayLock()
        {
            var pm = (PowerManager) Application.Context.GetSystemService(Context.PowerService);
            Tag += Guid.NewGuid().ToString();
            WakeLockInternal = pm.NewWakeLock(WakeLockFlags.Full, Tag);
            WakeLockInternal.Acquire();
        }

        public void Dispose()
        {
            WakeLockInternal?.Release();
            WakeLockInternal = null;
        }
    }
}