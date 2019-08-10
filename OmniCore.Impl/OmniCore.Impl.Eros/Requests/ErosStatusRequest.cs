using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using OmniCore.Model.Enums;
using OmniCore.Model.Interfaces;
using Exception = System.Exception;

namespace OmniCore.Impl.Eros.Requests
{
    public class ErosStatusRequest : IPodRequest
    {
        private StatusRequestType _requestType;
        public ErosStatusRequest(StatusRequestType requestType)
        {
            _requestType = requestType;
        }

        public async Task<IPodResult> Execute(IPod pod, IRadio radio)
        {
            var result = new ErosPodResult(ResultType.OK);
            return result;
        }

        public IList<IPodRequest> Enlist(IList<IPodRequest> pendingRequests)
        {
            return pendingRequests;
        }
    }
}
