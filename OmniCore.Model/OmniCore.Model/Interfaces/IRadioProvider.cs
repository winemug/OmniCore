using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmniCore.Model.Interfaces
{
    public interface IRadioProvider
    {
        IObservable<IRadio> ListRadios();
        Task<IRadio> GetByProviderSpecificId(string id);
    }
}
