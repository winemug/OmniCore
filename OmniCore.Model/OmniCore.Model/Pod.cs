using OmniCore.Model.Enums;
using OmniCore.Model.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace OmniCore.Model
{
    public abstract partial class Pod : IPod
    {
        public abstract Task UpdateStatus(StatusRequestType requestType);
        public abstract Task AcknowledgeAlerts(byte alertMask);
        public abstract Task Bolus(decimal bolusAmount);
        public abstract Task CancelBolus();
    }
}
