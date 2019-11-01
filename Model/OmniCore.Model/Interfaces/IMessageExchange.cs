using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OmniCore.Model.Interfaces
{
    public interface IMessageExchange
    {
        Task InitializeExchange();
        Task FinalizeExchange();
        Task<IMessage> GetResponse(IMessage request);

        string ActionText { get; set; }
        bool CanBeCanceled { get; set; }
        bool Waiting { get; set; }
        bool Running { get; set; }
        bool Finished { get; set; }
        int Progress { get; set; }
        CancellationToken Token { get; }
        void SetException(Exception exception);

        IMessageExchangeResult Result { get; }
        IMessageExchangeStatistics Statistics { get; }
        IMessageExchangeParameters Parameters { get; }
        IPod Pod { get; }
    }
}
