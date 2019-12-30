using System;
using OmniCore.Model.Interfaces.Services;

namespace OmniCore.Model.Interfaces.Services
{
    public interface IRadioService : ICoreService
    {
        string Description { get; }
        IObservable<IRadio> ListRadios();
    }
}