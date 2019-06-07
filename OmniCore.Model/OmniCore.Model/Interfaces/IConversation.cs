using OmniCore.Model.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OmniCore.Model.Interfaces
{
    public interface IConversation : INotifyPropertyChanged, IDisposable
    {
        bool CanCancel { get; set; }
        bool IsWaiting { get; set; }
        bool IsRunning { get; set; }
        bool IsFinished { get; set; }

        int Progress { get; set; }

        IMessageExchangeProgress NewExchange();

        CancellationToken Token { get; }
        Task<bool> Cancel();
        void CancelComplete();
        void CancelFailed();
    }
}
