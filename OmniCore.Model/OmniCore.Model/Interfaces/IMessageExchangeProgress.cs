using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace OmniCore.Model.Interfaces
{
    public interface IMessageExchangeProgress : INotifyPropertyChanged
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
    }
}
