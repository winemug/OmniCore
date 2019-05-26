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
        private readonly IMessageExchangeProvider MessageExchangeProvider;
        private SynchronizationContext MessageSynchronizationContext;

        public MessageHandler(IPod pod, IMessageExchangeProvider messageExchangeProvider)
        {
            Pod = pod;
            MessageExchangeProvider = messageExchangeProvider;
            MessageSynchronizationContext = new SynchronizationContext();
        }

        public async Task<PodCommandResult> PerformExchange(IMessage requestMessage, IMessageExchangeParameters messageExchangeParameters,
            IMessageProgress messageProgress, CancellationToken ct)
        {
            var previousContext = SynchronizationContext.Current;
            try
            {
                // SynchronizationContext.SetSynchronizationContext(MessageSynchronizationContext);
                var messageExchange = await MessageExchangeProvider.GetMessageExchanger(messageExchangeParameters, Pod, messageProgress, ct).ConfigureAwait(false);
                var response = await messageExchange.GetResponse(requestMessage, messageProgress, ct);
                return messageExchange.ParseResponse(response, Pod);
            }
            catch (Exception e)
            {
                return new PodCommandResult() { Success = false };
            }
            finally
            {
                // SynchronizationContext.SetSynchronizationContext(previousContext);
            }
        }
    }
}
