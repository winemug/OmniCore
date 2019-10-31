using System;
using System.Collections.Generic;
using System.Text;

namespace OmniCore.Client.Interfaces
{
    public interface IViewViewModel
    {
        IView View { get; }
        IViewModel ViewModel { get; }
    }
}
