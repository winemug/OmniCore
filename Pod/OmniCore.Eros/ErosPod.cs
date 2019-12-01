using System.Collections.Generic;
using System.Threading.Tasks;
using OmniCore.Model.Interfaces.Entities;
using OmniCore.Model.Interfaces.Workflow;

namespace OmniCore.Eros
{
    public class ErosPod : IPod
    {
        public IPodEntity Entity { get; set; }
        public Task Archive()
        {
            throw new System.NotImplementedException();
        }

        public Task<IList<IPodRequest>> GetActiveRequests()
        {
            throw new System.NotImplementedException();
        }

        public Task<IPodRequest> RequestPair()
        {
            throw new System.NotImplementedException();
        }

        public Task<IPodRequest> RequestPrime()
        {
            throw new System.NotImplementedException();
        }

        public Task<IPodRequest> RequestInsert()
        {
            throw new System.NotImplementedException();
        }

        public Task<IPodRequest> RequestStatus()
        {
            throw new System.NotImplementedException();
        }

        public Task<IPodRequest> RequestConfigureAlerts()
        {
            throw new System.NotImplementedException();
        }

        public Task<IPodRequest> RequestAcknowledgeAlerts()
        {
            throw new System.NotImplementedException();
        }

        public Task<IPodRequest> RequestSetBasalSchedule()
        {
            throw new System.NotImplementedException();
        }

        public Task<IPodRequest> RequestCancelBasal()
        {
            throw new System.NotImplementedException();
        }

        public Task<IPodRequest> RequestSetTempBasal()
        {
            throw new System.NotImplementedException();
        }

        public Task<IPodRequest> RequestCancelTempBasal()
        {
            throw new System.NotImplementedException();
        }

        public Task<IPodRequest> RequestBolus()
        {
            throw new System.NotImplementedException();
        }

        public Task<IPodRequest> RequestCancelBolus()
        {
            throw new System.NotImplementedException();
        }

        public Task<IPodRequest> RequestStartExtendedBolus()
        {
            throw new System.NotImplementedException();
        }

        public Task<IPodRequest> RequestCancelExtendedBolus()
        {
            throw new System.NotImplementedException();
        }

        public Task<IPodRequest> RequestDeactivate()
        {
            throw new System.NotImplementedException();
        }
    }
}