using OmniCore.Model.Protocol.Base;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OmniCore.Model.Protocol
{
    public class Conversation : IDisposable
    {
        //private static object ConversationLock = new object();
        private IMessageRadio Radio;
        private Pod Pod;
        private bool PendingAck = false;

        private Conversation(IMessageRadio radio, Pod pod)
        {
            this.Radio = radio;
            this.Pod = pod;
        }

        public static Conversation Start(IMessageRadio radio, Pod pod)
        {
            if (!pod.Address.HasValue)
                throw new ArgumentException("Pod address is unknown");

            //Monitor.Wait(ConversationLock);
            if (!radio.IsInitialized())
            {
                radio.InitializeRadio(pod.Address.Value);
                radio.SetNonceParameters(pod.Lot.Value, pod.Tid.Value, null, null);
            }
            return new Conversation(radio, pod);
        }

        public async Task<Message> SendRequest(Message request)
        {
            Message response = null;

            PendingAck = response != null;
            return response;
        }

        public void End()
        {
            if (PendingAck)
            {
                this.Radio.AcknowledgeResponse();
                this.PendingAck = false;
            }
            //Monitor.Exit(ConversationLock);
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                    this.End();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~Conversation() {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion
    }
}
