using OmniCore.Model.Interfaces;
using OmniCore.Repository.Enums;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace OmniCore.Radio.RileyLink
{
    public class RileyLinkRadio : IRadio
    {
        private IRadioPeripheral _radioPeripheral;
        public RileyLinkRadio(IRadioPeripheral radioPeripheral)
        {
            _radioPeripheral = radioPeripheral;
        }

        public string DeviceId
        {
            get
            {
                var gb = _radioPeripheral.PeripheralId.ToByteArray();
                return $"{gb[10]:X2}:{gb[11]:X2}:{gb[12]:X2}:{gb[13]:X2}:{gb[14]:X2}:{gb[15]:X2}";
            }
        }

        public string DeviceName => _radioPeripheral.PeripheralName;
        public string DeviceType => "RileyLink";
        public string ProviderSpecificId => "RLL" + _radioPeripheral.PeripheralId.ToString("N");
        public int Rssi => _radioPeripheral.Rssi;

        public Task Connect()
        {
            throw new NotImplementedException();
        }

        public Task Disconnect()
        {
            throw new NotImplementedException();
        }

        public Task PrepareForMessageExchange()
        {
            throw new NotImplementedException();
        }

        public Task<IMessage> ExchangeMessages(IMessage messageToSend, TxPower? TxLevel = null)
        {
            throw new NotImplementedException();
        }

    }
}
