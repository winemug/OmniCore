using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace OmniCore.Model
{
    public class ProtocolHandler
    {
        public Pod Pod { get; private set; }

        private Task CurrentExchange;

        internal ProtocolHandler(Pod pod)
        {
            CurrentExchange = Task.Run(() => { });
        }

        public async Task<PodMessage> PerformExchange(MessageExchange me)
        {
            return await me.GetPodResponse().ConfigureAwait(false);
        }
    }
}
