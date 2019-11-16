using OmniCore.Model.Interfaces.Platform;
using OmniCore.Repository.Enums;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OmniCore.Model.Interfaces
{
    public interface IRadioConnection : IDisposable
    {
        IRadioPeripheralLease PeripheralLease { get;  }
        Task<bool> Initialize(IRadioConfiguration configuration, CancellationToken cancellationToken);
        Task<IMessage> ExchangeMessages(IMessage messageToSend, CancellationToken cancellationToken, TxPower? TxLevel = null);
    }
}
