using OmniCore.Model.Interfaces.Entities;
using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OmniCore.Model.Interfaces.Operational
{
    public interface IRadioProvider
    {
        IObservable<IRadio> ListRadios(CancellationToken cancellationToken);
    }
}
