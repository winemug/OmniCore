using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using OmniCore.Model.Entities;
using OmniCore.Model.Enumerations;

namespace OmniCore.Model.Interfaces.Services
{
    public interface IPod: IDisposable
    {
        PodEntity Entity { get; set; }
        PodRunningState RunningState { get; }
        Task Archive(CancellationToken cancellationToken);
        IObservable<IPod> WhenPodArchived();
        Task<IList<IPodTask>> GetActiveRequests();
        Task<IPodTask> Start();
        Task<IPodTask> Status(StatusRequestType requestType);
        Task<IPodTask> ConfigureAlerts();
        Task<IPodTask> AcknowledgeAlerts();
        Task<IPodTask> SetBasalSchedule();
        Task<IPodTask> CancelBasal();
        Task<IPodTask> SetTempBasal();
        Task<IPodTask> CancelTempBasal();
        Task<IPodTask> Bolus();
        Task<IPodTask> CancelBolus();
        Task<IPodTask> StartExtendedBolus();
        Task<IPodTask> CancelExtendedBolus();
        Task<IPodTask> Deactivate();
        void StartMonitoring();
    }
}