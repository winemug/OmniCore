using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using OmniCore.Model.Entities;
using OmniCore.Model.Enumerations;
using OmniCore.Model.Interfaces.Common;
using OmniCore.Model.Interfaces.Services.Facade;
using OmniCore.Model.Interfaces.Services.Internal;
using OmniCore.Model.Utilities;

namespace OmniCore.Eros
{
    public class ErosPodRequest : ErosTask, IPodRequest
    {
        public PodRequestEntity Entity { get; set; }
        public IPod Pod { get; set; }

        private IErosPod ErosPod => Pod as IErosPod;

        private readonly List<(RequestPart part, ISubTaskProgress progress)> Parts =
            new List<(RequestPart part, ISubTaskProgress progress)>();

        private uint MessageSequence;
        private uint MessageAddress;
        private bool IsWithCriticalFollowup;
        private IErosRadio RadioOverride;

        public ErosPodRequest()
        {
        }

        protected override async Task ExecuteRequest(CancellationToken cancellationToken)
        {
            var radio = RadioOverride;
            if (radio == null)
            {
                //TODO: radio from radioentity
            }
            await radio.ExecuteRequest(this, cancellationToken);
        }

        public ErosPodRequest WithAcquire(IErosRadio radio)
        {
            var subProgress = Progress.AddSubProgress( "Query Pod", "Searching for pod");
                
            return this.WithPart(new RequestPart()
            {
                PartType = PartType.RequestStatus,
                PartData = new Bytes((byte)StatusRequestType.Standard)
            }, subProgress).WithRadio(radio);
        }

        private ErosPodRequest WithRadio(IErosRadio radio)
        {
            RadioOverride = radio;
            return this;
        }

        public ErosPodRequest WithPair(uint address)
        {
            var subProgress = Progress.AddSubProgress( "Pair Pod", "Pairing pod");
            
            return this.WithPart(new RequestPart()
            {
                PartType = PartType.RequestAssignAddress,
                PartData = new Bytes(address)
            }, subProgress);
        }

        public ErosPodRequest WithStatus(StatusRequestType requestType)
        {
            var subProgress = Progress.AddSubProgress( "Request Status", "Querying Pod Status");
            return this.WithPart(new RequestPart()
            {
                PartType = PartType.RequestStatus,
                PartData = new Bytes().Append((byte)requestType)
            }, subProgress);
        }

        private ErosPodRequest WithPart(RequestPart part, ISubTaskProgress subProgress)
        {
            Parts.Add((part, subProgress));
            return this;
        }

        public byte[] GetRequestData()
        {
            var messageBody = new Bytes();

            foreach (var (part, _) in Parts)
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