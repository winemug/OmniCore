using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Threading.Tasks;

namespace OmniCore.Client.Interfaces
{
    public interface IViewModel : INotifyPropertyChanged
    {
        Task Initialize();
        Task Dispose();
    }
}
