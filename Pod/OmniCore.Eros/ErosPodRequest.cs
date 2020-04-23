using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Tasks;
using OmniCore.Model.Entities;
using OmniCore.Model.Enumerations;
using OmniCore.Model.Exceptions;
using OmniCore.Model.Interfaces.Services;
using OmniCore.Model.Interfaces.Services.Internal;
using OmniCore.Model.Utilities;

namespace OmniCore.Eros
{
    public class ErosPodRequest : IErosPodRequest
    {
        private readonly List<RequestPart> Parts =
            new List<RequestPart>();
        private readonly IRepositoryService RepositoryService;

        private PodRequestEntity Entity;
        private int MessageSequence;
        private bool CriticalFollowup;
        private TransmissionPower? TransmissionPowerOverride;

        public IPod Pod => ErosPod;
        public IErosPod ErosPod { get; private set; }

        public byte[] Message => GetRequestData();
        public uint MessageAddress { get; private set; }
        public bool AllowAddressOverride { get; private set; }

        public ErosPodRequest(
            IRepositoryService repositoryService)
        {
            RepositoryService = repositoryService;
        }
        public IErosPodRequest WithPod(IErosPod pod)
        {
            ErosPod = pod;
            return this;
        }

        public IErosPodRequest WithEntity(PodRequestEntity entity)
        {
            Entity = entity;
            return this;
        }
        public IErosPodRequest WithMessageAddress(uint messageAddress)
        {
            MessageAddress = messageAddress;
            return this;
        }

        public IErosPodRequest WithMessageSequence(int messageSequence)
        {
            MessageSequence = messageSequence;
            return this;
        }

        public IErosPodRequest WithCriticalFollowup()
        {
            CriticalFollowup = true;
            return this;
        }

        public IErosPodRequest WithAllowAddressOverride()
        {
            AllowAddressOverride = true;
            return this;
        }

        public IErosPodRequest WithTransmissionPower(TransmissionPower transmissionPower)
        {
            TransmissionPowerOverride = transmissionPower;
            return this;
        }

        public IErosPodRequest WithStatusRequest(StatusRequestType requestType)
        {
            var childProgress = new TaskProgress
            {
                Name = "Request Status",
                Description = "Querying Pod Status"
            };

            return WithPart(new RequestPart
            {
                PartType = PartType.RequestStatus,
                PartData = new Bytes().Append((byte) requestType)
            });
        }

        public IErosPodRequest WithAcquireRequest()
        {
            AllowAddressOverride = true;
            TransmissionPowerOverride = TransmissionPower.Lowest;

            return WithPart(new RequestPart
            {
                PartType = PartType.RequestStatus,
                PartData = new Bytes((byte) StatusRequestType.Standard)
            });
        }

        public IErosPodRequest WithPairRequest(uint radioAddress)
        {
            return WithPart(new RequestPart
            {
                PartType = PartType.RequestAssignAddress,
                PartData = new Bytes(radioAddress)
            }).WithMessageAddress(0xffffffff);
        }

        private IErosPodRequest WithPart(RequestPart part)
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
            if (CriticalFollowup)
                b0 |= 0x80;
            b0 |= (byte) ((messageBody.Length >> 8) & 0x03);
            var b1 = (byte) (messageBody.Length & 0xff);

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