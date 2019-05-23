using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OmniCore.Model.Interfaces
{
    public interface IMessageExchangeProvider
    {
        Task<IMessageExchange> GetMessageExchanger(IMessageExchangeParameters messageExchangeParameters, IPod pod,
            IMessageProgress messageProgress, CancellationToken ct);
    }
}
