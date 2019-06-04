using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace OmniCore.Mobile.Interfaces
{
    public interface ILocalRequestSubscriber
    {
        Task OnRequestReceived(string request);
    }
}
