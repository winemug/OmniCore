using OmniCore.Model.Interfaces.Attributes;
using OmniCore.Model.Interfaces.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace OmniCore.Model.Interfaces.Operational
{
    public interface IPod
    {
        IPodEntity Entity { get; set; }
        Task Archive();
        Task<IList<IPodRequest>> GetActiveRequests();
        Task<IPodRequest> RequestPair();
        Task<IPodRequest> RequestPrime();
        Task<IPodRequest> RequestInsert();
        Task<IPodRequest> RequestStatus();
        Task<IPodRequest> RequestConfigureAlerts();
        Task<IPodRequest> RequestAcknowledgeAlerts();
        Task<IPodRequest> RequestSetBasalSchedule();
        Task<IPodRequest> RequestCancelBasal();
        Task<IPodRequest> RequestSetTempBasal();
        Task<IPodRequest> RequestCancelTempBasal();
        Task<IPodRequest> RequestBolus();
        Task<IPodRequest> RequestCancelBolus();
        Task<IPodRequest> RequestStartExtendedBolus();
        Task<IPodRequest> RequestCancelExtendedBolus();
        Task<IPodRequest> RequestDeactivate();
    }
}
