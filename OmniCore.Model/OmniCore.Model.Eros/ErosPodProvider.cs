using OmniCore.Model.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using OmniCore.Mobile.Base;
using Xamarin.Forms;

namespace OmniCore.Model.Eros
{
    public class ErosPodProvider : IPodProvider
    {
        private List<IRadioProvider> RadioProviders;

        public ErosPodProvider()
        {
            RadioProviders = new List<IRadioProvider>()
            {
                new RileyLinkRadioProvider()
            };
        }

        public async Task<IPod> GetActivePod()
        {
            var repo = await ErosRepository.GetInstance();
            return await repo.GetActivePod();
        }

        public async Task<IEnumerable<IPod>> GetActivePods()
        {
            var repo = await ErosRepository.GetInstance();
            return await repo.GetActivePods();
        }

        public async Task Archive(IPod pod)
        {
            pod.Archived = true;
            var repo = await ErosRepository.GetInstance();
            await repo.Save(pod as ErosPod);
            MessagingCenter.Send<IPodProvider>(this, MessagingConstants.PodsChanged);
        }

        public async Task<IPod> New()
        {
            var pod = new ErosPod
            {
                Id = Guid.NewGuid()
            };

            pod.RadioAddress = GetRadioAddress();
            pod.Created = DateTimeOffset.UtcNow;
            var repo = await ErosRepository.GetInstance();
            await repo.Save(pod);
            MessagingCenter.Send<IPodProvider>(this, MessagingConstants.PodsChanged);
            return pod;
        }

        public async Task<IPod> Register(uint lot, uint serial, uint radioAddress)
        {
            var pod = new ErosPod() { Id = Guid.NewGuid(), Lot = lot, Serial = serial, RadioAddress = radioAddress };
            pod.Created = DateTimeOffset.UtcNow;
            var repo = await ErosRepository.GetInstance();
            await repo.Save(pod);
            MessagingCenter.Send<IPodProvider>(this, MessagingConstants.PodsChanged);
            return pod;
        }

        public Task<IConversation> StartConversation(IPod pod)
        {
            throw new NotImplementedException();
        }

        public Task CancelConversations(IPod pod)
        {
            throw new NotImplementedException();
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
