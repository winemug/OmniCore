using OmniCore.Model.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AppCenter.Crashes;
using OmniCore.Mobile.Base;
using OmniCore.Mobile.Base.Interfaces;
using OmniCore.Model.Enums;
using OmniCore.Model.Exceptions;
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

        private async Task<IRadioProvider> GetRadioProvider(IPod pod)
        {
            return RadioProviders[0];
        }

        public async Task<IConversation> StartConversation(IPod pod,
            string intent,
            int timeout = 0,
            RequestSource source = RequestSource.OmniCoreUser)
        {
            int started = Environment.TickCount;
            while (!OmniCoreServices.AppState.TrySet(AppStateConstants.ActiveConversation, intent))
            {
                if (timeout > 0 && Environment.TickCount - started > timeout)
                    throw new OmniCoreTimeoutException(FailureType.OperationInProgress, "Timed out waiting for existing operation to complete");
                await Task.Delay(250);
            }

            IConversation conversation = null;
            IWakeLock wakeLock = null;
            try
            {
                wakeLock = OmniCoreServices.Application.NewBluetoothWakeLock(
                    Guid.NewGuid().ToString()
                        .Replace('-', '_')
                        .Replace('{', '_')
                        .Replace('}', '_')
                );

                var ret = await wakeLock.Acquire(10000);
                if (!ret)
                {
                    wakeLock.Release();
                    throw new OmniCoreException(FailureType.WakeLockNotAcquired);
                }

                conversation = new ErosConversation(wakeLock, GetRadioProvider(pod), pod) { RequestSource = source, Intent = intent };
                MessagingCenter.Send<IConversation>(conversation, MessagingConstants.ConversationStarted);
                return conversation;
            }
            catch(Exception e)
            {
                Crashes.TrackError(e);
                wakeLock?.Dispose();
                OmniCoreServices.AppState.TryRemove(AppStateConstants.ActiveConversation);
                conversation?.Dispose();
                throw;
            }
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
