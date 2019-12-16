using OmniCore.Model.Interfaces.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using OmniCore.Model.Constants;
using OmniCore.Model.Interfaces.Platform;
using OmniCore.Model.Interfaces.Repositories;
using OmniCore.Model.Interfaces.Workflow;
using Unity;

namespace OmniCore.Radios.RileyLink
{
    public class RileyLinkRadio : IRadio
    {
        public IRadioPeripheral Peripheral { get; set; }
        public IRadioEntity Entity { get; set; }
        public IRadioConfiguration DefaultConfiguration { get => new RadioConfiguration(); }

        public IRadioConfiguration GetConfiguration()
        {
            var json = Entity.ConfigurationJson;
            if (string.IsNullOrEmpty(json))
                return DefaultConfiguration;
            return JsonConvert.DeserializeObject<RadioConfiguration>(json);
        }

        public async Task SetConfiguration(IRadioConfiguration configuration)
        {
            Entity.ConfigurationJson = JsonConvert.SerializeObject(configuration);
            await RadioRepository.Update(Entity, CancellationToken.None);
        }

        private readonly IRadioAdapter Adapter;
        private readonly IUnityContainer Container;
        private readonly IRadioRepository RadioRepository;
        public RileyLinkRadio(IRadioAdapter adapter,
            IUnityContainer container,
            IRadioRepository radioRepository)
        {
            Adapter = adapter;
            Container = container;
            RadioRepository = radioRepository;
        }
        public async Task<IRadioLease> Lease(CancellationToken cancellationToken)
        {
            var peripheralLease = await Peripheral.Lease(cancellationToken);
            IsBusy = true;
            var radioLease = Container.Resolve<IRadioLease>(RegistrationConstants.RileyLink);
            radioLease.PeripheralLease = peripheralLease;
            radioLease.Radio = this;
            return radioLease;
        }

        public bool IsBusy { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
