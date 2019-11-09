using OmniCore.Model.Interfaces;
using OmniCore.Repository.Entities;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OmniCore.Radios.RileyLink
{
    public class RileyLinkLeaseManager
    {
        private Guid PeripheralId;
        private RileyLinkRadioConnection RileyLinkRadioConnection;
        private IRadioAdapter RadioAdapter;
        private SemaphoreSlim RadioSemaphore;

        private static Dictionary<Guid,RileyLinkLeaseManager> LeaseManagers
            = new Dictionary<Guid, RileyLinkLeaseManager>();


        public static async Task<RileyLinkLeaseManager> GetManager(IRadioAdapter radioAdapter, Guid peripheralId)
        {
            lock(LeaseManagers)
            {
                if (!LeaseManagers.ContainsKey(peripheralId))
                {
                    LeaseManagers.Add(peripheralId, new RileyLinkLeaseManager(radioAdapter, peripheralId));
                }
                return LeaseManagers[peripheralId];
            }
        }

        private RileyLinkLeaseManager(IRadioAdapter radioAdapter, Guid peripheralId)
        {
            RadioAdapter = radioAdapter;
            PeripheralId = peripheralId;
            RadioSemaphore = new SemaphoreSlim(1,1);
        }

        public async Task<IRadioLease> Acquire(PodRequest request, CancellationToken cancellationToken)
        {
            RileyLinkRadioLease lease = null;
            try
            {
                await RadioSemaphore.WaitAsync(cancellationToken);
            }
            catch(OperationCanceledException)
            {
                return null;
            }

            try
            {
                if (RileyLinkRadioConnection == null)
                {
                    var peripheral = await RadioAdapter.GetPeripheral(PeripheralId, cancellationToken);
                    if (peripheral == null)
                        return null;
                    RileyLinkRadioConnection = new RileyLinkRadioConnection(peripheral);
                }
            }
            catch(OperationCanceledException) { }
            finally
            {
                RadioSemaphore.Release();
            }
            return lease;
        }

        public async Task Release()
        {
        }
    }
}
