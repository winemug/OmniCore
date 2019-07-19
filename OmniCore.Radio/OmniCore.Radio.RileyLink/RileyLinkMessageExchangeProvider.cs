using OmniCore.Model.Enums;
using OmniCore.Model.Eros;
using OmniCore.Model.Interfaces;
using OmniCore.Model.Interfaces.Data;
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
        public async Task<IMessageExchange> GetMessageExchange(IMessageExchangeParameters messageExchangeParameters, IPod pod, IMessageExchangeProgress messageProgress)
        {
            if (RileyLinkInstance != null)
            {
                // Make sure this instance is still valid
                if (!await RileyLinkInstance.DeviceIsValid(messageProgress))
                {
                    if (messageProgress != null)
                        messageProgress.ActionText = "Reconnecting to RileyLink";

                    // Dispose() and create new RileyLinkInstance
                    RileyLinkInstance = null;
                    // Dispose() and create new RileyLinkMessageExchange
                    RileyLinkMessageExchange = null;
                }
            }

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
