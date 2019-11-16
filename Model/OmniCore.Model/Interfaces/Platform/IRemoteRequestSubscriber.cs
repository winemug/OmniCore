using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace OmniCore.Model.Interfaces.Platform
{
    public interface IRemoteRequestSubscriber
    {
        Task<string> OnRequestReceived(string request);
    }
}
