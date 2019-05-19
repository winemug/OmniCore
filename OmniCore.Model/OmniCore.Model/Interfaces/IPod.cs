using System;
using System.Threading.Tasks;
using OmniCore.Model.Enums;

namespace OmniCore.Model.Interfaces
{
    public interface IPod : IPodVariables
    {
        Task AcknowledgeAlerts(byte alert_mask);
        Task Bolus(decimal bolusAmount);
        Task CancelBolus();
        Task UpdateStatus(byte update_type = 0);
    }
}