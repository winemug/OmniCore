using OmniCore.Model.Interfaces.Data;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OmniCore.Model.Interfaces
{
    public interface IMessageExchange
    {
        Task InitializeExchange(IMessageExchangeProgress messageProgress);
        Task FinalizeExchange();
        Task<IMessage> GetResponse(IMessage request, IMessageExchangeProgress messageProgress);
        void ParseResponse(IMessage response, IPod pod, IMessageExchangeProgress messageProgress);
    }
}
