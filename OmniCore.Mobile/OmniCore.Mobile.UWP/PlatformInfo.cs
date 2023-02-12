using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OmniCore.Services.Interfaces;

namespace OmniCore.Mobile.UWP
{
    public class PlatformInfo : IPlatformInfo
    {
        public string SoftwareVersion => "1.0";
        public string HardwareVersion => "1.0";
        public string Platform => "UWP";
        public string OsVersion => "1.0";
        public bool HasAllPermissions => true;
        public bool IsExemptFromBatteryOptimizations => true;

        public Task<bool> RequestMissingPermissions()
        {
            return Task.FromResult(true);
        }

        public void OpenBatteryOptimizationSettings()
        {
        }
    }
}
