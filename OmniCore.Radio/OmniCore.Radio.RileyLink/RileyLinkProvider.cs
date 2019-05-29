using OmniCore.Model.Eros;
using OmniCore.Model.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OmniCore.Radio.RileyLink
{
    public class RileyLinkProvider : IMessageExchangeProvider
    {
        private static RileyLink RileyLinkInstance;
        private static RileyLinkMessageExchange RileyLinkMessageExchange;
        private readonly SynchronizationContext UiSyncContext;

        public RileyLinkProvider(SynchronizationContext uiSyncContext)
        {
            UiSyncContext = uiSyncContext;
        }

        public async Task<IMessageExchange> GetMessageExchanger(IMessageExchangeParameters messageExchangeParameters, IPod pod)
        {
            if (RileyLinkInstance == null)
            {
                RileyLinkInstance = new RileyLink();
            }

            if (RileyLinkMessageExchange == null)
                RileyLinkMessageExchange = new RileyLinkMessageExchange(messageExchangeParameters, pod, RileyLinkInstance, UiSyncContext);
            else
                RileyLinkMessageExchange.UpdateParameters(messageExchangeParameters, pod, RileyLinkInstance);

            return RileyLinkMessageExchange;
        }
    }
}
