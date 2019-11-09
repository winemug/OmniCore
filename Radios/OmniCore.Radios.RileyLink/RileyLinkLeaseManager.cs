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
        private Radio RadioEntity;
        private RileyLinkRadioConnection RileyLinkRadioConnection;
        private IRadioAdapter RadioAdapter;
        private SemaphoreSlim RadioSemaphore;

        private static Dictionary<string,RileyLinkLeaseManager> LeaseManagers
            = new Dictionary<string, RileyLinkLeaseManager>();


        public static async Task<RileyLinkLeaseManager> GetManager(IRadioAdapter radioAdapter, Radio radioEntity)
        {
            lock(LeaseManagers)
            {
                if (!LeaseManagers.ContainsKey(radioEntity.ProviderSpecificId))
                {
                    LeaseManagers.Add(radioEntity.ProviderSpecificId, new RileyLinkLeaseManager(radioAdapter, radioEntity));
                }
                return LeaseManagers[radioEntity.ProviderSpecificId];
            }
        }

        private RileyLinkLeaseManager(IRadioAdapter radioAdapter, Radio radioEntity)
        {
            RadioAdapter = radioAdapter;
            RadioEntity = radioEntity;
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
                    var peripheral = await RadioAdapter.GetPeripheral(RadioEntity.DeviceId, cancellationToken);
                    if (peripheral == null)
                        return null;
                    RileyLinkRadioConnection = await RileyLinkRadioConnection.CreateInstance(peripheral, RadioEntity, request);
                }
            }
            catch(OperationCanceledException)
            {
                RileyLinkRadioConnection?.Dispose();
            }
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
