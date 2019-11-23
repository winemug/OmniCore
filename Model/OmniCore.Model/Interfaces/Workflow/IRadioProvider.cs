using System;
using System.Threading;

namespace OmniCore.Model.Interfaces.Workflow
{
    public interface IRadioProvider
    {
        IObservable<IRadio> ListRadios(CancellationToken cancellationToken);
    }
}