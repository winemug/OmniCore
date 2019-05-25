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
        private IPod Pod;
        private Task CurrentExchange;
        private readonly IMessageExchangeProvider MessageExchangeProvider;

        public MessageHandler(IPod pod, IMessageExchangeProvider messageExchangeProvider)
        {
            Pod = pod;
            MessageExchangeProvider = messageExchangeProvider;
            CurrentExchange = Task.Run(() => { });
        }

        public async Task<PodCommandResult> PerformExchange(IMessage requestMessage, IMessageExchangeParameters messageExchangeParameters,
            IMessageProgress messageProgress, CancellationToken ct)
        {
            try
            {
                var messageExchange = await MessageExchangeProvider.GetMessageExchanger(messageExchangeParameters, Pod, messageProgress, ct);
                var response = await messageExchange.GetResponse(requestMessage, messageProgress, ct);
                return messageExchange.ParseResponse(response, Pod);
            }
            catch (Exception e)
            {
                return new PodCommandResult() { Success = false };
            }
        }
    }
}
