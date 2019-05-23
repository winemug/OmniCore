using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace OmniCore.Model.Interfaces
{
    public interface IMessageProgress : INotifyPropertyChanged
    {
        bool CanBeCanceled { get; }
        bool Queued { get; }
        bool Running { get; }
        int Progress { get; }
        bool Finished { get; }
        bool Successful { get; }
    }
}
