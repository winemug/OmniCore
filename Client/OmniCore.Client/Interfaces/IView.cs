using System;
using System.Collections.Generic;
using System.Text;

namespace OmniCore.Client.Interfaces
{
    public interface IView<T> where T : IViewModel
    {
        T ViewModel { get; set; }
    }
}
