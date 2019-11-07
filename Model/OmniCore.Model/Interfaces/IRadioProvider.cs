using OmniCore.Repository.Entities;
using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmniCore.Model.Interfaces
{
    public interface IRadioProvider
    {
        IObservable<Radio> ListRadios();
        Task<IRadioPeripheral> GetByProviderSpecificId(string id);
    }
}
