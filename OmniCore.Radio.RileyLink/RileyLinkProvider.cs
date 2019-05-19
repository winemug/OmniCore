using OmniCore.Model.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace OmniCore.Radio.RileyLink
{
    public class RileyLinkProvider : IMessageExchangeProvider
    {
        public Task<IMessageExchange> GetMessageExchanger()
        {
            throw new NotImplementedException();
        }
    }
}
