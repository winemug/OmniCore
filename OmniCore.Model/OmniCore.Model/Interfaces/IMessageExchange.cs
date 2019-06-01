using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OmniCore.Model.Interfaces
{
    public interface IMessageExchange
    {
        Task InitializeExchange(IMessageExchangeProgress messageProgress, CancellationToken ct);
        Task<IMessage> GetResponse(IMessage request, IMessageExchangeProgress messageProgress, CancellationToken ct);
        IMessageExchangeResult ParseResponse(IMessage response, IPod pod);
    }
}
