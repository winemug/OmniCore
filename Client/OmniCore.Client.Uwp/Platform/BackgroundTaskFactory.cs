using OmniCore.Model.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmniCore.Client.Uwp.Platform
{
    public class BackgroundTaskFactory : IBackgroundTaskFactory
    {
        public IBackgroundTask CreateBackgroundTask(Action action)
        {
            throw new NotImplementedException();
        }
    }
}
