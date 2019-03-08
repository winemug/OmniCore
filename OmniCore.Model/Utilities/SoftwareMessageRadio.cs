using OmniCore.Model.Protocol.Base;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace OmniCore.Model.Utilities
{
    public class SoftwareMessageRadio : IMessageRadio
    {
        private readonly IPacketRadio PacketRadio;

        private uint Address;
        private int PacketSequence = 0;
        private int MessageSequence = 0;
        private bool Initialized = false;

        private NonceGenerator NonceGenerator;

        public SoftwareMessageRadio(IPacketRadio packetRadio)
        {
            this.PacketRadio = packetRadio;
        }

        public bool IsInitialized()
        {
            return Initialized;
        }

        public async Task InitializeRadio(uint address)
        {
            this.Address = address;
            await this.PacketRadio.Initialize();
            this.Initialized = true;
        }

        public async Task AcknowledgeResponse()
        {
            throw new NotImplementedException();
        }

        public void ResetCounters()
        {
            this.PacketSequence = 0;
            this.MessageSequence = 0;
        }

        public async Task<IMessage> SendRequestAndGetResponse(IMessage request)
        {
            request.Parts.Where(p => p.NonceRequired).ToList()
                .ForEach(p => p.SetNonce(this.NonceGenerator.GetNext()));

            var content = request.GetMessageData();
            int contentLen = content.Length;
            var data = new byte[contentLen + 2];
            data[0] = request.WillFollowUpWithCriticalRequest ? (byte)0x80 : (byte)0x00;
            data[0] |= (byte)(GetNextMessageSequence() << 2);
            data[0] |= (byte)((contentLen >> 8) & 0x03);
            data[1] = (byte)(contentLen & 0xFF);
            Buffer.BlockCopy(content, 0, data, 2, contentLen);

            var packets = new List<byte[]>();
            byte[] packetData;
            int index = GetPacket(data, 0, out packetData);
            while(packetData != null)
            {
                packets.Add(packetData);
                index = GetPacket(data, index, out packetData);
            }

            var responsePackets = new List<byte[]>();
            foreach(var packet in packets)
            {
                var response = await this.PacketRadio.SendPacketAndGetPacket(packet);
            }

            throw new NotImplementedException();
        }

        private int GetPacket(byte[] data, int startIndex, out byte[] packetData)
        {
            int dataLength = data.Length - startIndex;
            if (dataLength <= 0)
            {
                packetData = null;
                return -1;
            }
            int packetDataLength = Math.Min(dataLength, startIndex == 0 ? 27 : 31);
            if (startIndex == 0)
            {
                packetData = new byte[packetDataLength + 10];
                packetData.PutUint32BigEndian(this.Address, 0);
                packetData[4] = 0xA0;
                packetData[4] |= GetNextPacketSequence();
                packetData.PutUint32BigEndian(this.Address, 5);
                Buffer.BlockCopy(data, startIndex, packetData, 9, packetDataLength);
            }
            else
            {
                packetData = new byte[packetDataLength + 6];
                packetData.PutUint32BigEndian(this.Address, 0);
                packetData[4] = 0x80;
                packetData[4] |=  GetNextPacketSequence();
                Buffer.BlockCopy(data, startIndex, packetData, 5, packetDataLength);
            }
            return startIndex + packetDataLength;
        }

        private byte GetNextPacketSequence()
        {
            var seq = this.PacketSequence;
            this.PacketSequence = (this.PacketSequence + 1) % 32;
            return (byte)seq;
        }

        private byte GetNextMessageSequence()
        {
            var seq = this.MessageSequence;
            this.MessageSequence = (this.MessageSequence + 1) % 16;
            return (byte)seq;
        }

        public async Task SetLowTxLevel()
        {
            this.PacketRadio.SetLowTx();
        }
        public async Task SetNormalTxLevel()
        {
            this.PacketRadio.SetNormalTx();
        }

        public void SetNonceParameters(uint lot, uint tid, uint? nonce, int? seed)
        {
            this.NonceGenerator = new NonceGenerator(lot, tid, nonce, seed);
        }
    }
}
