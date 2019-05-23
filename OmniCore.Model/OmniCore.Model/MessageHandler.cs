using OmniCore.Model.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OmniCore.Model
{
    public class MessageHandler
    {
        public Pod Pod { get; private set; }

        private Task CurrentExchange;
        private readonly IMessageExchangeProvider MessageExchangeProvider;

        public MessageHandler(IMessageExchangeProvider messageExchangeProvider)
        {
            MessageExchangeProvider = messageExchangeProvider;
            CurrentExchange = Task.Run(() => { });
        }

        public async Task<IMessage> PerformExchange(IMessage requestMessage, IMessageExchangeParameters messageExchangeParameters,
            IMessageProgress messageProgress, CancellationToken ct)
        {
            var messageExchange = await MessageExchangeProvider.GetMessageExchanger(messageExchangeParameters, this.Pod, messageProgress, ct).ConfigureAwait(false);
            return await messageExchange.GetResponse(requestMessage, messageProgress, ct);
        }
    }
}
