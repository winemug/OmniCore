using System;
using System.Collections.Generic;
using System.Text;
using OmniCore.Model.Interfaces;

namespace OmniCore.Impl.Eros.Tests
{
    class TestRadioPeripheral : IRadioPeripheral
    {
        public int Rssi { get; }
        public Guid PeripheralId { get; }
        public string PeripheralName { get; }
    }
}
