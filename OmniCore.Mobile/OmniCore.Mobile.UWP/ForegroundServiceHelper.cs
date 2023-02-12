using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OmniCore.Services.Interfaces;
using Unity;

namespace OmniCore.Mobile.UWP
{
    public class ForegroundServiceHelper : IForegroundServiceHelper
    {
        public void StartForegroundService()
        {
            OmniCore.Mobile.App.Container.Resolve<ICoreService>().Start();
        }

        public void StopForegroundService()
        {
            OmniCore.Mobile.App.Container.Resolve<ICoreService>().Stop();
        }
    }
}
