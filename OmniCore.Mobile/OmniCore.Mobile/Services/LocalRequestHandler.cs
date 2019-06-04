using OmniCore.Mobile.Interfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;

namespace OmniCore.Mobile.Services
{
    public class LocalRequestHandler : ILocalRequestSubscriber
    {
        public async Task OnRequestReceived(string request)
        {
            Debug.WriteLine($"BROADCAST REQUEST: {request}");
        }
    }
}
