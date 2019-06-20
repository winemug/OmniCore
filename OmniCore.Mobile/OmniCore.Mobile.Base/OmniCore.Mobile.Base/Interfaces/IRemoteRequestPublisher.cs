using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace OmniCore.Mobile.Base.Interfaces
{
    public interface IRemoteRequestPublisher
    {
        Task<string> GetResult(string request);
        void Subscribe(IRemoteRequestSubscriber subscriber);
        void Unsubscribe(IRemoteRequestSubscriber subscriber);
    }
}
