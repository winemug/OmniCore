using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using OmniCore.Model.Interfaces.Common;
using OmniCore.Services;

namespace OmniCore.Client.Droid
{
    public class AapsIntegrationServiceBase : OmniCoreServiceBase, ICoreIntegrationService
    {
        protected override Task OnStart(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        protected override Task OnStop(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        protected override Task OnPause(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        protected override Task OnResume(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}