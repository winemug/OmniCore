using OmniCore.Model.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace OmniCore.Model
{
    public class ProtocolHandler
    {
        public Pod Pod { get; private set; }

        private Task CurrentExchange;
        private readonly IMessageExchangeProvider MessageExchangeProvider;

        public ProtocolHandler(IMessageExchangeProvider messageExchangeProvider)
        {
            MessageExchangeProvider = messageExchangeProvider;
            CurrentExchange = Task.Run(() => { });
        }

        public async Task<IResponse> PerformExchange(IRequest requestMessage, IMessageProgress messageProgress = null)
        {
            var messageExchange = await MessageExchangeProvider.GetMessageExchanger().ConfigureAwait(false);
            return await messageExchange.GetResponse(requestMessage, messageProgress);
        }
    }
}
