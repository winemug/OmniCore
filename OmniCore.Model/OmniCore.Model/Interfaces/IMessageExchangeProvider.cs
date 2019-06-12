using OmniCore.Model.Enums;
using OmniCore.Model.Interfaces.Data;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OmniCore.Model.Interfaces
{
    public interface IMessageExchangeProvider
    {
        Task<IMessageExchange> GetMessageExchange(IMessageExchangeParameters messageExchangeParameters, IPod pod);
    }
}
