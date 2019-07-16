using OmniCore.Model.Enums;
using OmniCore.Model.Interfaces.Data;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OmniCore.Model.Interfaces
{
    public interface IConversation : IDisposable
    {
        DateTimeOffset Started { get; set; }
        DateTimeOffset? Ended { get; set; }
        string Intent { get; set; }

        bool CanCancel { get; set; }
        bool IsRunning { get; set; }
        bool IsFinished { get; set; }

        Exception Exception { get; set; }
        FailureType FailureType { get; set; }
        bool Failed { get; set; }
        bool Canceled { get; set; }

        IMessageExchangeProgress NewExchange(IMessage requestMessage);
        RequestSource RequestSource { get; set; }
        IMessageExchangeProgress CurrentExchange { get; set; }

        CancellationToken Token { get; }
        Task<bool> Cancel();
        void CancelComplete();
        void CancelFailed();
    }
}
