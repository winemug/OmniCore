using OmniCore.Model.Eros.Data;
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
        public IEnumerable<IPod> Pods { get; set; }

        public IPod SinglePod => Pods?.FirstOrDefault();

        public ErosPodProvider()
        {
            Pods = new List<ErosPod>();
        }

        public async Task Initialize()
        {
            var repo = await ErosRepository.GetInstance();
            Pods = await repo.GetActivePods();
            MessagingCenter.Send<IPodProvider>(this, MessagingConstants.PodsChanged);
        }

        public async Task Archive(IPod pod)
        {
            pod.Archived = true;
            var repo = await ErosRepository.GetInstance();
            await repo.Save(pod as ErosPod);
            Pods = await repo.GetActivePods();
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
            Pods = await repo.GetActivePods();
            MessagingCenter.Send<IPodProvider>(this, MessagingConstants.PodsChanged);
            return pod;
        }

        public async Task<IPod> Register(uint lot, uint serial, uint radioAddress)
        {
            var pod = new ErosPod() { Id = Guid.NewGuid(), Lot = lot, Serial = serial, RadioAddress = radioAddress };
            pod.Created = DateTimeOffset.UtcNow;
            var repo = await ErosRepository.GetInstance();
            await repo.Save(pod);
            Pods = await repo.GetActivePods();
            MessagingCenter.Send<IPodProvider>(this, MessagingConstants.PodsChanged);
            return pod;
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
