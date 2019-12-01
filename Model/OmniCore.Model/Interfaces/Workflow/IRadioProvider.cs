using System;
using System.Threading;

namespace OmniCore.Model.Interfaces.Workflow
{
    public interface IRadioProvider
    {
        string Description { get; }
        string Code { get; }
        IPodProvider[] PodProviders { get; }
        IObservable<IRadio> ListRadios(CancellationToken cancellationToken);
    }
}