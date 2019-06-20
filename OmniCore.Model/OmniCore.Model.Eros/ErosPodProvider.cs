using OmniCore.Model.Eros.Data;
using OmniCore.Model.Interfaces;
using System;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using System.Text;

namespace OmniCore.Model.Eros
{
    public class ErosPodProvider : IPodProvider
    {
        readonly IMessageExchangeProvider MessageExchangeProvider;

        private IPodManager _PodManager;
        public IPodManager PodManager
        {
            get
            {
                return _PodManager;
            }
            private set
            {
                if (_PodManager != value)
                {
                    _PodManager = value;
                    ManagerChanged?.Invoke(this, new EventArgs());
                }
            }
        }

        public event EventHandler ManagerChanged;

        public ErosPodProvider(IMessageExchangeProvider messageExchangeProvider)
        {
            MessageExchangeProvider = messageExchangeProvider;
            var pod = ErosRepository.Instance.LoadCurrent();
            if (pod != null)
            {
                PodManager = new PodManagerAsyncProxy(new ErosPodManager(pod, MessageExchangeProvider));
            }
        }

        public void Archive()
        {
            lock (this)
            {
                if (PodManager != null)
                {
                    PodManager.Pod.Archived = true;
                    ErosRepository.Instance.Save(PodManager.Pod as ErosPod);
                    PodManager = null;
                }
            }
        }

        public IPodManager New()
        {
            lock (this)
            {
                Archive();

                var pod = new ErosPod
                {
                    Id = Guid.NewGuid()
                };

                pod.RadioAddress = GetRadioAddress();
                pod.Created = DateTimeOffset.UtcNow;
                ErosRepository.Instance.Save(pod);
                PodManager = new PodManagerAsyncProxy(new ErosPodManager(pod, MessageExchangeProvider));
                return PodManager;
            }
        }

        public IPodManager Register(uint lot, uint serial, uint radioAddress)
        {
            lock (this)
            {
                Archive();

                var pod = new ErosPod() { Id = Guid.NewGuid(), Lot = lot, Serial = serial, RadioAddress = radioAddress };
                pod.Created = DateTimeOffset.UtcNow;
                ErosRepository.Instance.Save(pod);
                PodManager = new PodManagerAsyncProxy(new ErosPodManager(pod, MessageExchangeProvider));
                return PodManager;
            }
        }

        private uint GetRadioAddress()
        {
            var random = new Random();
            var buffer = new byte[3];
            random.NextBytes(buffer);
            uint address = 0x34000000;
            address |= (uint)buffer[0] << 16;
            address |= (uint)buffer[1] << 8;
            address |= (uint)buffer[2];
            return address;
        }
    }
}
