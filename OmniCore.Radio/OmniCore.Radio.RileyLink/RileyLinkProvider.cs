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

        public async Task<IMessageExchange> GetMessageExchanger(IMessageExchangeParameters messageExchangeParameters, IPod pod,
            IMessageProgress messageProgress, CancellationToken ct)
        {
            if (RileyLinkInstance == null)
                RileyLinkInstance = new RileyLink();

            var rme = new RileyLinkMessageExchange(messageExchangeParameters, pod, RileyLinkInstance);
            await rme.InitializeExchange(messageProgress, ct);
            return rme;
        }
    }
}
