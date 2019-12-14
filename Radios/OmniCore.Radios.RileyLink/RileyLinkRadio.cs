using OmniCore.Model.Interfaces.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using OmniCore.Model.Constants;
using OmniCore.Model.Interfaces.Platform;
using OmniCore.Model.Interfaces.Workflow;
using Unity;

namespace OmniCore.Radios.RileyLink
{
    public class RileyLinkRadio : IRadio
    {
        public IRadioPeripheral Peripheral { get; set; }
        public IRadioEntity Entity { get; set; }
        
        private readonly IRadioAdapter Adapter;
        private readonly IUnityContainer Container;
        public RileyLinkRadio(IRadioAdapter adapter,
            IUnityContainer container)
        {
            Adapter = adapter;
            Container = container;
        }

        public async Task<IRadioConfiguration> GetDefaultConfiguration()
        {
            return new RadioConfiguration();
        }

        public async Task<IRadioLease> Lease(CancellationToken cancellationToken)
        {
            var peripheralLease = await Peripheral.Lease(cancellationToken);
            var radioLease = Container.Resolve<IRadioLease>(RegistrationConstants.RileyLink);
            radioLease.PeripheralLease = peripheralLease;
            radioLease.Radio = this;
            return radioLease;
        }
    }
}
