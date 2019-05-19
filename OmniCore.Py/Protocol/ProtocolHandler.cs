using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace OmniCore.Py.Protocol
{
    public class ProtocolHandler
    {
        public Pod Pod { get; private set; }

        private Task CurrentExchange;

        public ProtocolHandler(Pod pod)
        {
            Pod = pod ?? throw new ArgumentNullException();
            CurrentExchange = Task.Run(() => { });
        }

        public async Task<PodMessage> PerformExchange(MessageExchange me)
        {
            return await me.GetPodResponse();
        }

    }
}
