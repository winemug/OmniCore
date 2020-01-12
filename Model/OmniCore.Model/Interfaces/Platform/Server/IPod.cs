using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using OmniCore.Model.Enumerations;
using OmniCore.Model.Interfaces.Platform.Common;
using OmniCore.Model.Interfaces.Platform.Common.Data.Entities;

namespace OmniCore.Model.Interfaces.Platform.Common
{
    public interface IPod : IServerResolvable
    {
        IPodEntity Entity { get; set; }
        IPodRequest ActiveRequest { get; }
        Task Archive();
        Task<IList<IPodRequest>> GetActiveRequests();
        Task<IPodRequest> RequestPair(uint radioAddress);
        Task<IPodRequest> RequestPrime();
        Task<IPodRequest> RequestInsert();
        Task<IPodRequest> RequestStatus(StatusRequestType requestType);
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