using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using OmniCore.Model.Entities;
using OmniCore.Model.Enumerations;
using OmniCore.Model.Interfaces.Platform.Common;
using OmniCore.Model.Utilities;

namespace OmniCore.Eros
{
    public class ErosPodRequest : ErosTask, IPodRequest
    {
        public PodRequestEntity Entity { get; set; }
        public IPod Pod { get; set; }

        private readonly List<RequestPart> Parts = new List<RequestPart>();

        private uint MessageSequence;
        private uint MessageAddress;
        private bool IsWithCriticalFollowup;

        private readonly IRadioService RadioService;
        public ErosPodRequest(IRadioService radioService)
        {
            RadioService = radioService;
        }

        protected override async Task ExecuteRequest(CancellationToken cancellationToken)
        {
            // TODO: Pod.Radio
        }

        public ErosPodRequest WithPair(uint address)
        {
            return this.WithPart(new RequestPart()
            {
                PartType = PartType.RequestAssignAddress,
                PartData = new Bytes(address)
            });
        }

        public ErosPodRequest WithStatus(StatusRequestType requestType)
        {
            return this.WithPart(new RequestPart()
            {
                PartType = PartType.RequestStatus,
                PartData = new Bytes().Append((byte)requestType)
            });
        }

        private ErosPodRequest WithPart(RequestPart part)
        {
            Parts.Add(part);
            return this;
        }

        public byte[] GetRequestData()
        {
            var messageBody = new Bytes();

            foreach (var part in Parts)
            {
                messageBody.Append((byte) part.PartType);

                var partBody = new Bytes();
                if (part.RequiresNonce)
                    partBody.Append(GetNonce());
                partBody.Append(part.PartData);

                var partBodyLength = (byte) partBody.Length;

                messageBody.Append(partBodyLength);
                messageBody.Append(partBody);
            }

            var b0 = (byte) (MessageSequence << 2);
            if (IsWithCriticalFollowup)
                b0 |= 0x80;
            b0 |= (byte)((messageBody.Length >> 8) & 0x03);
            var b1 = (byte)(messageBody.Length & 0xff);

            var requestBody = new Bytes(MessageAddress).Append(b0).Append(b1).Append(messageBody);

            return new Bytes(requestBody).Append(CrcUtil.Crc16(requestBody)).ToArray();
        }

        private uint GetNonce()
        {
            //TODO:
            return 0;
        }
    }
}