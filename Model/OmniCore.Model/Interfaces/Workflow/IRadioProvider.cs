using System;
using System.Threading;

namespace OmniCore.Model.Interfaces.Workflow
{
    public interface IRadioProvider
    {
        string Description { get; }
        IPodProvider[] PodProviders { get; }
        IObservable<IRadio> ListRadios(CancellationToken cancellationToken);
    }
}