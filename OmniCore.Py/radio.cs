using Omni.Py;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OmniCore.Py
{

    public class Radio
    {
        public IPacketRadio packet_radio;

        private logger logger = definitions.getLogger();
        private logger packet_logger = definitions.get_packet_logger();

        public Pod Pod { get; set; }

        public Radio(IPacketRadio pr, Pod pod)
        {
            this.Pod = Pod;
            this.packet_radio = pr;
        }

        public async Task<PodMessage> SendAndGet(PdmMessage message)
        {
            var me = new MessageExchange(message, this.packet_radio, this.Pod);
            return await me.GetPodResponse();
        }

    }
}