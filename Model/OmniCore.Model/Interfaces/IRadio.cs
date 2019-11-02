using OmniCore.Repository.Enums;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace OmniCore.Model.Interfaces
{
    public interface IRadio
    {
        string DeviceId { get; }
        string DeviceName { get; }
        string DeviceType { get; }
        string ProviderSpecificId { get; }
        Task Connect();
        Task Disconnect();
        int Rssi { get; }
        Task PrepareForMessageExchange();
        Task<IMessage> ExchangeMessages(IMessage messageToSend, TxPower? TxLevel = null);
    }
}
