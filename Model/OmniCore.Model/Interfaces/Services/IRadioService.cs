using System;
using OmniCore.Model.Interfaces;

namespace OmniCore.Model.Interfaces
{
    public interface IRadioService : ICoreService
    {
        string Description { get; }
        IObservable<IRadio> ListRadios();
    }
}