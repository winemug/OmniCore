using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OmniCore.Model.Interfaces
{
    public interface IMessageExchangeProgress : INotifyPropertyChanged, IDisposable
    {
        string CommandText { get; set; }
        string ActionText { get; set; }
        string ActionStatusText { get; set; }

        bool CanBeCanceled { get; set; }

        bool Waiting { get; set; }
        bool Running { get; set; }
        bool Finished { get; set; }
        bool Successful { get; set; }

        int OutgoingSuccess { get; set; }
        int OutgoingFail { get; set; }
        int IncomingSuccess { get; set; }
        int IncomingFail { get; set; }

        int Progress { get; set; }

        IMessageExchangeStatistics Statistics { get; set; }

        CancellationToken Token { get; }

        Task<bool> CancelExchange();
        void CancelComplete();
        void CancelFailed();
    }
}
