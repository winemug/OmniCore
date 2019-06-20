using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace OmniCore.Mobile.Base.Interfaces
{
    public interface IRemoteRequestSubscriber
    {
        Task<string> OnRequestReceived(string request);
    }
}
