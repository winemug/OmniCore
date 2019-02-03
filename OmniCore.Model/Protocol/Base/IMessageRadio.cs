using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace OmniCore.Model.Protocol.Base
{
    public interface IMessageRadio
    {
        bool IsInitialized();
        Task InitializeRadio(uint address);
        void SetNonceParameters(uint lot, uint tid, uint? nonce, int? seed);
        void ResetCounters();
        Task SetNormalTxLevel();
        Task SetLowTxLevel();
        Task<IMessage> SendRequestAndGetResponse(IMessage request);
        Task AcknowledgeResponse();
    }
}
