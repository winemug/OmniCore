using Newtonsoft.Json;
using OmniCore.Model.Enums;
using OmniCore.Model.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OmniCore.Impl.Eros.Requests
{
    public class ErosRequestPair : ErosRequest, IPodRequestPair
    {
        public uint RadioAddress { get; set;  }

        public override RequestType PodRequestType => RequestType.Pair;

        public override IList<IPodRequest> Enlist(IList<IPodRequest> pendingRequests)
        {
            throw new NotImplementedException();
        }

        protected override Task<IPodResult> OnExecute(IPod pod, IRadio radio, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
