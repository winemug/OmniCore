using OmniCore.Model.Enums;
using OmniCore.Model.Eros;
using OmniCore.Model.Interfaces;
using OmniCore.Model.Interfaces.Data;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OmniCore.Radio.RileyLink
{
    public class RileyLinkMessageExchangeProvider : IMessageExchangeProvider
    {
        private static RileyLink RileyLinkInstance;
        private static RileyLinkMessageExchange RileyLinkMessageExchange;

        public async Task<IMessageExchange> GetMessageExchange(IMessageExchangeParameters messageExchangeParameters, IPod pod)
        {
            if (RileyLinkInstance == null)
            {
                RileyLinkInstance = new RileyLink(ErosRepository.Instance.GetRadioPreferences());
            }

            if (RileyLinkMessageExchange == null)
                RileyLinkMessageExchange = new RileyLinkMessageExchange(messageExchangeParameters, pod, RileyLinkInstance);
            else
                RileyLinkMessageExchange.SetParameters(messageExchangeParameters, pod, RileyLinkInstance);

            return RileyLinkMessageExchange;
        }

    }
}
