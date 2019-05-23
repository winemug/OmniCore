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
        public async Task<IMessageExchange> GetMessageExchanger(IMessageExchangeParameters messageExchangeParameters, IPod pod,
            IMessageProgress messageProgress, CancellationToken ct)
        {
            var rme = new RileyLinkMessageExchange(messageExchangeParameters, pod);
            await rme.InitializeExchange(messageProgress, ct);
            return rme;
        }
    }
}
