using OmniCore.Model.Enums;
using OmniCore.Model.Eros;
using OmniCore.Model.Interfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using OmniCore.Model.Eros.Data;

namespace OmniCore.Radio.RileyLink
{
    public class RileyLinkMessageExchangeProvider : IMessageExchangeProvider
    {
        private Dictionary<Guid, RileyLinkMessageExchange> LastExchanges;

        public RileyLinkMessageExchangeProvider()
        {
            LastExchanges = new Dictionary<Guid, RileyLinkMessageExchange>();
        }

        [method: SuppressMessage("", "CS1998", Justification = "Not applicable")]
        public async Task<IMessageExchange> GetMessageExchange(IMessageExchangeParameters messageExchangeParameters, CancellationToken token, IPod pod)
        {

            var exchange = new RileyLinkMessageExchange(messageExchangeParameters, token, pod);
            if (LastExchanges.ContainsKey(pod.Id))
            {
                var lastExchange = LastExchanges[pod.Id];
                await lastExchange.FinalizeExchange();
                exchange.RileyLink = new RileyLink(lastExchange.RileyLink, exchange);
            }
            await exchange.InitializeExchange();
            LastExchanges[pod.Id] = exchange;
            return exchange;
        }

    }
}
