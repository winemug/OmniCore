using OmniCore.Model.Enums;
using OmniCore.Model.Eros;
using OmniCore.Model.Interfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OmniCore.Radio.RileyLink
{
    public class RileyLinkMessageExchangeProvider : IMessageExchangeProvider
    {
        private static RileyLink RileyLinkInstance;
        private static RileyLinkMessageExchange RileyLinkMessageExchange;

        [method: SuppressMessage("", "CS1998", Justification = "Not applicable")]
        public async Task<IMessageExchange> GetMessageExchange(IMessageExchangeParameters messageExchangeParameters, IPod pod)
        {
            if (RileyLinkInstance == null)
            {
                var repo = await ErosRepository.GetInstance();
                RileyLinkInstance = new RileyLink(await repo.GetRadioPreferences());
            }

            if (RileyLinkMessageExchange == null)
                RileyLinkMessageExchange = new RileyLinkMessageExchange(messageExchangeParameters, pod, RileyLinkInstance);
            else
                RileyLinkMessageExchange.SetParameters(messageExchangeParameters, pod, RileyLinkInstance);

            return RileyLinkMessageExchange;
        }

    }
}
