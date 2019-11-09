using OmniCore.Repository.Enums;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace OmniCore.Model.Interfaces
{
    public interface IRadioConnection : IDisposable
    {
        string DeviceId { get; }
        string DeviceName { get; }
        string DeviceType { get; }
        string ProviderSpecificId { get; }

        Task<bool> Connect();
        Task Disconnect();
        Task<bool> PrepareForMessageExchange();
        Task<IMessage> ExchangeMessages(IMessage messageToSend, TxPower? TxLevel = null);
    }
}
