using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using OmniCore.Model.Entities;
using OmniCore.Model.Enumerations;
using OmniCore.Model.Interfaces.Common;

namespace OmniCore.Model.Interfaces.Services.Facade
{
    public interface IPod : IServiceInstance, IDisposable
    {
        PodEntity Entity { get; set; }
        PodRunningState RunningState { get; }
        Task Archive();
        Task<IList<IPodRequest>> GetActiveRequests();
        Task<IPodRequest> Start();
        Task<IPodRequest> Status(StatusRequestType requestType);
        Task<IPodRequest> ConfigureAlerts();
        Task<IPodRequest> AcknowledgeAlerts();
        Task<IPodRequest> SetBasalSchedule();
        Task<IPodRequest> CancelBasal();
        Task<IPodRequest> SetTempBasal();
        Task<IPodRequest> CancelTempBasal();
        Task<IPodRequest> Bolus();
        Task<IPodRequest> CancelBolus();
        Task<IPodRequest> StartExtendedBolus();
        Task<IPodRequest> CancelExtendedBolus();
        Task<IPodRequest> Deactivate();
    }
}