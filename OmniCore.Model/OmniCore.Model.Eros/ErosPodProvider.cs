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

        IPodManager PodManager;

        public event EventHandler PodChanged;

        public ErosPodProvider(IMessageExchangeProvider messageExchangeProvider)
        {
            MessageExchangeProvider = messageExchangeProvider;
        }

        private void RaisePodChanged()
        {
            PodChanged?.Invoke(this, new EventArgs());
        }

        public void Archive()
        {
            if (Current != null)
            {
                PodManager.Pod.Archived = true;
                ErosRepository.Instance.Save(PodManager.Pod as ErosPod);
                PodManager = null;
                RaisePodChanged();
            }
        }

        public IPodManager Current
        {
            get
            {
                lock (this)
                {
                    if (PodManager == null)
                    {
                        var pod = ErosRepository.Instance.LoadCurrent();
                        if (pod != null)
                        {
                            PodManager = new ErosPodManager(pod, MessageExchangeProvider);
                        }
                    }
                    return PodManager;
                }
            }
        }

        public IPodManager New()
        {
            lock (this)
            {
                if (Current != null)
                {
                    Archive();
                    RaisePodChanged();
                }

                var pod = new ErosPod
                {
                    Id = Guid.NewGuid()
                };

                PodManager = new ErosPodManager(pod, MessageExchangeProvider);
                //var lastActivatedPod = ErosRepository.Instance.GetLastActivated();
                //if (lastActivatedPod != null)
                //{
                //    var lastRadioAddress = lastActivatedPod.RadioAddress;
                //    pod.RadioAddress = (lastRadioAddress & 0xFFFFFFF0) | (((lastRadioAddress & 0x0000000F) + 1) & 0x0000000F);
                //}
                //else
                //{
                    pod.RadioAddress = GetRadioAddress();
                //}
                pod.Created = DateTime.UtcNow;
                ErosRepository.Instance.Save(pod);
                RaisePodChanged();
                return Current;
            }
        }

        public IPodManager Register(uint lot, uint serial, uint radioAddress)
        {
            lock (this)
            {
                if (Current != null)
                {
                    Archive();
                    RaisePodChanged();
                }

                var pod = new ErosPod() { Id = Guid.NewGuid(), Lot = lot, Serial = serial, RadioAddress = radioAddress };
                pod.Created = DateTime.UtcNow;
                ErosRepository.Instance.Save(pod);
                RaisePodChanged();
                return Current;
            }
        }

        private uint GetRadioAddress()
        {
            //foreach (var ni in NetworkInterface.GetAllNetworkInterfaces())
            //{
            //    if (ni.NetworkInterfaceType == NetworkInterfaceType.Wireless80211)
            //    {
            //        var addressBytes = ni.GetPhysicalAddress().GetAddressBytes();
            //        Array.Reverse(addressBytes);
            //        return (uint)0x20000000 | (uint)(addressBytes[0] << 20) | (uint)(addressBytes[1] << 12) | (uint)(addressBytes[2] << 4);

            //    }
            //}
            //return (uint)0x21721720;
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
