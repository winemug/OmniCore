using Omni.Py;
using System;
using System.Threading.Tasks;

namespace OmniCore.Py
{
    public class MessageExchange
    {
        public int unique_packets = 0;
        public int repeated_sends = 0;
        public int receive_timeouts = 0;
        public int repeated_receives = 0;
        public int protocol_errors = 0;
        public int bad_packets = 0;
        public int radio_errors = 0;
        public bool successful = false;
        public DateTime Started;
        public DateTime Ended;

        public PdmMessage PdmMessage;
        public PodMessage PodMessage;
        public Exception Error;

        private IPacketRadio _packetRadio;
        private Pod _pod;
        public MessageExchange(PdmMessage pdmMessage, IPacketRadio packetRadio, Pod pod)
        {
            this.PdmMessage = pdmMessage;
            this._packetRadio = packetRadio;
            this._pod = pod;
        }

        public async Task<PodMessage> GetPodResponseAsync()
        {
            this.Started = DateTime.UtcNow;
            return await ExchangeMessages();
        }

        private async Task<PodMessage> ExchangeMessages()
        {
            if (this.PdmMessage.TxLevel.HasValue)
            {

            }
            return null;
        }
    }
}