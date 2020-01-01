using System.Collections.Generic;
using System.Threading.Tasks;
using OmniCore.Model.Interfaces.Data.Entities;

namespace OmniCore.Model.Interfaces.Platform
{
    public interface IPod
    {
        IPodEntity Entity { get; set; }
        IPodRequest ActiveRequest { get; }
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
        Task StartQueue();
        Task StopQueue();
    }
}