using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OmniCore.Model.Interfaces
{
    public interface IMessageExchangeHandler
    {
        Task<IMessageExchangeResult> PerformExchange(IMessage requestMessage, IMessageExchangeParameters messageExchangeParameters,
            IMessageProgress messageProgress, CancellationToken ct);
    }
}
