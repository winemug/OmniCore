using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace OmniCore.Model.Interfaces
{
    public interface IRadioPeripheral
    {
        Guid PeripheralId { get; }
        string PeripheralName { get; }
        Task<bool> IsConnected();
    }
}
