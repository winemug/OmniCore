using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OmniCore.Impl.Eros
{
    public class ErosRequestProcessor
    {
        private List<ErosRequest> Request { get; }
        public CancellationTokenSource CancellationSource { get; }
        public TaskCompletionSource<ErosResult> ResultSource { get; }

        public bool Executing { get; }
        private Guid _podId;

        public ErosRequestProcessor(Guid podId)
        {
            _podId = podId;
            CancellationSource = new CancellationTokenSource();
            ResultSource = new TaskCompletionSource<ErosResult>();
        }
    }
}
