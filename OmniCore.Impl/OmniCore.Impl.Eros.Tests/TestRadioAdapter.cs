using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using OmniCore.Model.Interfaces;

namespace OmniCore.Impl.Eros.Tests
{
    class TestRadioAdapter : IRadioAdapter
    {
        public Task<bool> TryEnable()
        {
            throw new NotImplementedException();
        }

        public Task<bool> TryDisable()
        {
            throw new NotImplementedException();
        }

        public IObservable<IRadioPeripheral> ScanPeripherals(Guid serviceId)
        {
            throw new NotImplementedException();
        }

        public Task<IRadioPeripheral> GetPeripheral(Guid id)
        {
            throw new NotImplementedException();
        }
    }
}
