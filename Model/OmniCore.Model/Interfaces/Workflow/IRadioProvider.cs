using System;
using System.Threading;

namespace OmniCore.Model.Interfaces.Workflow
{
    public interface IRadioProvider
    {
        string Description { get; }
        IObservable<IRadio> ListRadios();
    }
}