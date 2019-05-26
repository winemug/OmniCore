using OmniCore.Model.Enums;
using OmniCore.Model.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OmniCore.Model
{
    public abstract partial class Pod : IPod
    {
        public abstract Task UpdateStatus(IMessageProgress progress, CancellationToken ct, StatusRequestType requestType);
        public abstract Task AcknowledgeAlerts(IMessageProgress progress, CancellationToken ct, byte alertMask);
        public abstract Task Bolus(IMessageProgress progress, CancellationToken ct, decimal bolusAmount);
        public abstract Task CancelBolus(IMessageProgress progress, CancellationToken ct);

        public async Task<bool> WithLotAndTid(uint lot, uint tid)
        {
            this.Lot = lot;
            this.Serial = tid;
            return await DataStore.Load(this);
        }

        private readonly IDataStore DataStore;

        public Pod(IDataStore dataStore)
        {
            DataStore = dataStore;
        }
    }
}
