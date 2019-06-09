using OmniCore.Mobile.Interfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;

namespace OmniCore.Mobile.Services
{
    public class RemoteRequestHandler : IRemoteRequestSubscriber
    {
        public async Task<string> OnRequestReceived(string request)
        {
            return "{}";
        }
    }
}
