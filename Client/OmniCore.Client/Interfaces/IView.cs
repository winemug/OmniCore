using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace OmniCore.Client.Interfaces
{
    public interface IView<out T> where T : IViewModel
    {
        T ViewModel { get;  }
    }
}
