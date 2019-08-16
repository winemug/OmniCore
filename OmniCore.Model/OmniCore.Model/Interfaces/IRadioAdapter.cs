using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace OmniCore.Model.Interfaces
{
    public interface IRadioAdapter
    {
        Task<bool> TryEnable();
        Task<bool> TryDisable();
        IObservable<IRadioPeripheral> ScanPeripherals(Guid serviceId);
        Task<IRadioPeripheral> GetPeripheral(Guid id);
    }
}
