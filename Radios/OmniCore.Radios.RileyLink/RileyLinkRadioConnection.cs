using OmniCore.Model.Interfaces;
using OmniCore.Repository.Enums;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace OmniCore.Radios.RileyLink
{
    public class RileyLinkRadioConnection : IRadioConnection
    {
        private IRadioPeripheral Peripheral;
        public RileyLinkRadioConnection(IRadioPeripheral radioPeripheral)
        {
            Peripheral = radioPeripheral;
        }

        public string DeviceId
        {
            get
            {
                var gb = Peripheral.PeripheralId.ToByteArray();
                return $"{gb[10]:X2}:{gb[11]:X2}:{gb[12]:X2}:{gb[13]:X2}:{gb[14]:X2}:{gb[15]:X2}";
            }
        }

        public string DeviceName => Peripheral.PeripheralName;
        public string DeviceType => "RileyLink";
        public string ProviderSpecificId => "RLL" + Peripheral.PeripheralId.ToString("N");

        public async Task<bool> Connect()
        {
            throw new NotImplementedException();
        }

        public async Task Disconnect()
        {
            throw new NotImplementedException();
        }

        public async Task<bool> PrepareForMessageExchange()
        {
            throw new NotImplementedException();
        }

        public Task<IMessage> ExchangeMessages(IMessage messageToSend, TxPower? TxLevel = null)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}
