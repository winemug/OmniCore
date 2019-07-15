using OmniCore.Model.Eros.Data;
using OmniCore.Model.Interfaces;
using System;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;

namespace OmniCore.Model.Eros
{
    public class ErosPodProvider : IPodProvider
    {
        readonly IMessageExchangeProvider MessageExchangeProvider;

        private IPodManager _podManager;
        public IPodManager PodManager
        {
            get
            {
                return _podManager;
            }
            private set
            {
                if (_podManager != value)
                {
                    _podManager = value;
                    ManagerChanged?.Invoke(this, new EventArgs());
                }
            }
        }

        public event EventHandler ManagerChanged;

        public ErosPodProvider(IMessageExchangeProvider messageExchangeProvider)
        {
            MessageExchangeProvider = messageExchangeProvider;
        }

        public async Task Initialize()
        {
            var repo = await ErosRepository.GetInstance();
            var pod = await repo.LoadCurrent();
            if (pod != null)
            {
                PodManager = new PodManagerAsyncProxy(new ErosPodManager(pod, MessageExchangeProvider));
            }
        }

        public async Task Archive()
        {
            if (PodManager != null)
            {
                PodManager.Pod.Archived = true;
                var repo = await ErosRepository.GetInstance();
                await repo.Save(PodManager.Pod as ErosPod);
                PodManager = null;
            }
        }

        public async Task<IPodManager> New()
        {
            await Archive();

            var pod = new ErosPod
            {
                Id = Guid.NewGuid()
            };

            pod.RadioAddress = GetRadioAddress();
            pod.Created = DateTimeOffset.UtcNow;
            var repo = await ErosRepository.GetInstance();
            await repo.Save(pod);
            PodManager = new PodManagerAsyncProxy(new ErosPodManager(pod, MessageExchangeProvider));
            return PodManager;
        }

        public async Task<IPodManager> Register(uint lot, uint serial, uint radioAddress)
        {
            await Archive();

            var pod = new ErosPod() { Id = Guid.NewGuid(), Lot = lot, Serial = serial, RadioAddress = radioAddress };
            pod.Created = DateTimeOffset.UtcNow;
            var repo = await ErosRepository.GetInstance();
            await repo.Save(pod);
            PodManager = new PodManagerAsyncProxy(new ErosPodManager(pod, MessageExchangeProvider));
            return PodManager;
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
