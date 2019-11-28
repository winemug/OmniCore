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
            throw new NotImplementedException();
        }

        //public async Task<IRadioConnection> GetConnection(Radio radioEntity, PodRequest request, CancellationToken cancellationToken)
        //{
        //    var peripheralLease = await RadioAdapter.LeasePeripheral(radioEntity.DeviceId, cancellationToken);
        //    if (peripheralLease == null)
        //        return null;

        //    return new RileyLinkRadioConnection(peripheralLease, radioEntity, request);

        //}
        public async Task<IRadioConnection> GetConnection(IRadioConfiguration radioConfiguration, CancellationToken cancellationToken)
        {
            var lease = await Adapter.LeasePeripheral(Entity.DeviceUuid, cancellationToken);
            if (lease != null)
            {
                var connection = Container.Resolve<IRadioConnection>(RegistrationConstants.RileyLinkRadioConnection);
                connection.Lease = lease;
                connection.Radio = this;

                return connection;

            }

            return null;
        }

    }
}
