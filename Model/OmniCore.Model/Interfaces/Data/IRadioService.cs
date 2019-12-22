using System;
using OmniCore.Model.Interfaces.Platform;

namespace OmniCore.Model.Interfaces.Data
{
    public interface IRadioService
    {
        string Description { get; }
        IObservable<IRadio> ListRadios();
    }
}