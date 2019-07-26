using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace OmniCore.Model.Interfaces
{
    public interface IRadio
    {
        Task Connect();
        Task Disconnect();
        Task<int> GetRssi();
        Task<IMessage> ExchangeMessages(IMessage messageToSend);
    }
}
