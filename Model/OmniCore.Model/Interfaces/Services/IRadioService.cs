using System;
using OmniCore.Model.Interfaces.Platform;

namespace OmniCore.Model.Interfaces.Platform
{
    public interface IRadioService : ICoreService
    {
        string Description { get; }
        IObservable<IRadio> ListRadios();
    }
}