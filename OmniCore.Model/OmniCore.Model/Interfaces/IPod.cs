using System;
using System.Threading.Tasks;
using OmniCore.Model.Enums;

namespace OmniCore.Model.Interfaces
{
    public interface IPod : IPodVariables
    {
        Task AcknowledgeAlerts(byte alertMask);
        Task Bolus(decimal bolusAmount);
        Task CancelBolus();
        Task UpdateStatus(StatusRequestType requestType);
    }
}