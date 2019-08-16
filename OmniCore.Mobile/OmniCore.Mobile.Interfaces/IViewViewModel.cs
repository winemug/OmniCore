using System;
using System.Collections.Generic;
using System.Text;

namespace OmniCore.Mobile.Interfaces
{
    public interface IViewViewModel
    {
        IView View { get; }
        IViewModel ViewModel { get; }
    }
}
