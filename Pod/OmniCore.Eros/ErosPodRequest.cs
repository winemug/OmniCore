using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Tasks;
using OmniCore.Model.Entities;
using OmniCore.Model.Enumerations;
using OmniCore.Model.Exceptions;
using OmniCore.Model.Interfaces.Common;
using OmniCore.Model.Interfaces.Services;
using OmniCore.Model.Interfaces.Services.Facade;
using OmniCore.Model.Interfaces.Services.Internal;
using OmniCore.Model.Utilities;

namespace OmniCore.Eros
{
    public class ErosPodRequest : IErosPodRequest
    {
        private readonly ISubject<bool> CanCancelSubject = new BehaviorSubject<bool>(true);
        private readonly List<(RequestPart part, ITaskProgress progress)> Parts =
            new List<(RequestPart part, ITaskProgress progress)>();
        private readonly ISubject<TaskResult> ResultSubject = new AsyncSubject<TaskResult>();
        private readonly ISubject<TaskState> StateSubject = new BehaviorSubject<TaskState>(TaskState.Scheduled);
        private readonly CancellationTokenSource TaskCancellationSource = new CancellationTokenSource();
        private readonly IRepositoryService RepositoryService;

        private IErosPod ErosPod;
        private IErosRadio RadioOverride;
        private PodRequestEntity Entity;
        private int MessageSequence;
        private bool CriticalFollowup;
        private TransmissionPower? TransmissionPowerOverride;

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
            }, childProgress);
        }

        public IErosPodRequest WithAcquireRequest(IErosRadio radio)
        {
            var childProgress = new TaskProgress
            {
                Name = "Query Pod",
                Description = "Looking for pod"
            };

            AllowAddressOverride = true;
            TransmissionPowerOverride = TransmissionPower.Lowest;

            return WithPart(new RequestPart
            {
                PartType = PartType.RequestStatus,
                PartData = new Bytes((byte) StatusRequestType.Standard)
            }, childProgress).WithRadio(radio);
        }

        public IErosPodRequest WithPairRequest(uint radioAddress, IErosRadio radio)
        {
            var childProgress = new TaskProgress
            {
                Name = "Pair Pod",
                Description = "Pairing pod"
            };

            return WithPart(new RequestPart
            {
                PartType = PartType.RequestAssignAddress,
                PartData = new Bytes(radioAddress)
            }, childProgress);
        }
        public void Cancel()
        {
            TaskCancellationSource.Cancel();
        }

        public IObservable<bool> WhenCanCancelChanged()
        {
            return CanCancelSubject.AsObservable();
        }

        public IObservable<TaskState> WhenStateChanged()
        {
            return StateSubject.AsObservable();
        }

        public IObservable<TaskResult> WhenResultReceived()
        {
            return ResultSubject.AsObservable();
        }

        public async Task ExecuteRequest()
        {
            StateSubject.OnNext(TaskState.Running);
            try
            {
                await ExecuteRequestInternal(TaskCancellationSource.Token);
            }
            catch (OperationCanceledException)
            {
                ResultSubject.OnNext(TaskResult.Canceled);
                ResultSubject.OnCompleted();
            }
            catch (Exception e)
            {
                ResultSubject.OnError(e);
                ResultSubject.OnNext(TaskResult.Failed);
                ResultSubject.OnCompleted();
            }

            StateSubject.OnNext(TaskState.Finished);
            StateSubject.OnCompleted();
        }

        public void Dispose()
        {
            TaskCancellationSource?.Dispose();
        }

        private async Task ExecuteRequestInternal(CancellationToken cancellationToken)
        {
            var radio = RadioOverride;
            if (radio == null)
            {
                //TODO: radio from radioentity
            }

            var options = radio.Options;

            if (TransmissionPowerOverride.HasValue)
                options.Amplification = TransmissionPowerOverride.Value;

            var response = await radio.GetResponse(this, cancellationToken, options);
            var responseEntity = await ParseResponse(response);

            using var context = await RepositoryService.GetContextReadWrite(cancellationToken);
            Entity.Responses.Add(responseEntity);
            await context.Save(cancellationToken);
        }

        private ErosPodRequest WithRadio(IErosRadio radio)
        {
            RadioOverride = radio;
            return this;
        }

        private ErosPodRequest WithPart(RequestPart part, ITaskProgress taskProgress)
        {
            Progress.Children.Add(taskProgress);
            Parts.Add((part, taskProgress));
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

        private async Task<PodResponseEntity> ParseResponse(byte[] responseData)
        {
            var response = new PodResponseEntity
            {
                PodRequest = Entity
            };

            var responseBytes = new Bytes(responseData);

            var responseRadioAddress = responseBytes.DWord(0);

            var responseParts = new List<(byte, Bytes)>();
            var idx = 6;
            while (idx < responseBytes.Length - 2)
            {
                var responsePartCode = responseBytes[idx];
                var responsePartLength = responseBytes[idx + 1];
                var responseresponseData = responseBytes.Sub(idx + 2, idx + 2 + responsePartLength);
                responseParts.Add((responsePartCode, responseresponseData));
            }

            foreach (var (responsePartCode, responseresponseData) in responseParts)
                switch ((PartType) responsePartCode)
                {
                    case PartType.ResponseVersionInfo:
                        ParseVersionResponse(responseresponseData, response);
                        break;
                    case PartType.ResponseDetailInfoResponse:
                        //parse_information_response(pod, result);
                        break;
                    case PartType.ResponseResyncResponse:
                        //parse_resync_response(pod as ErosPod, result);
                        break;
                    case PartType.ResponseStatus:
                        ParseStatusResponse(responseresponseData, response);
                        break;
                    default:
                        throw new OmniCoreWorkflowException(FailureType.WorkflowPodResponseUnrecognized,
                            $"Unknown response type {responsePartCode}");
                }

            using var context = await RepositoryService.GetContextReadWrite(CancellationToken.None);
            await context.PodResponses.AddAsync(response);
            await context.Save(CancellationToken.None);
            return response;
        }

        private void ParseVersionResponse(Bytes responseData, PodResponseEntity response)
        {
            response.VersionResponse = new PodResponseVersion();

            var lengthyResponse = false;
            var i = 0;
            if (responseData.Length == 27)
            {
                response.VersionResponse.VersionUnk2b = responseData.ByteBuffer[new Range(i, i + 7)];
                i += 7;
                lengthyResponse = true;
            }

            var mx = responseData.Byte(i++);
            var my = responseData.Byte(i++);
            var mz = responseData.Byte(i++);
            response.VersionResponse.VersionPm = $"{mx}.{my}.{mz}";

            var ix = responseData.Byte(i++);
            var iy = responseData.Byte(i++);
            var iz = responseData.Byte(i++);
            response.VersionResponse.VersionPi = $"{ix}.{iy}.{iz}";

            i++;

            response.Progress = (PodProgress) (responseData.Byte(i++) & 0x0F);


            response.VersionResponse.Lot = responseData.DWord(i);
            response.VersionResponse.Serial = responseData.DWord(i + 4);
            i += 8;
            if (!lengthyResponse)
            {
                var rb = responseData.Byte(i++);
                response.RadioResponse = new PodResponseRadio
                {
                    PodLowGain = (byte) (rb >> 6),
                    PodRssi = (byte) (rb & 0b00111111)
                };
            }

            response.VersionResponse.RadioAddress = responseData.DWord(i);
        }

        private void ParseStatusResponse(Bytes responseData, PodResponseEntity response)
        {
            response.StatusResponse = new PodResponseStatus();
            var s0 = responseData[0];
            var s1 = responseData.DWord(1);
            var s2 = responseData.DWord(5);

            var deliveryStates = ParseDeliveryStates((byte) (s0 >> 4));
            response.StatusResponse.BolusState = deliveryStates.bolusState;
            response.StatusResponse.BasalState = deliveryStates.basalState;
            response.Progress = (PodProgress) (s0 & 0xF);

            response.StatusResponse.MessageSequence = (int) (s1 & 0x00007800) >> 11;
            response.StatusResponse.Delivered = (int) (s1 & 0x0FFF8000) >> 15;
            response.StatusResponse.NotDelivered = (int) s1 & 0x000007FF;
            response.StatusResponse.Faulted = s2 >> 31 != 0;
            response.StatusResponse.AlertMask = (byte) ((s2 >> 23) & 0xFF);
            response.StatusResponse.ActiveMinutes = (int) (s2 & 0x007FFC00) >> 10;
            response.StatusResponse.Reservoir = (int) s2 & 0x000003FF;
        }

        private void ParseInformationResponse(Bytes responseData, PodResponseEntity response)
        {
            var i = 0;
            var rt = responseData.Byte(i++);
            switch (rt)
            {
                //case 0x01:
                //    var alrs = new ErosAlertStates();

                //    alrs.AlertW278 = responseData.Word(i);
                //    i += 2;
                //    alrs.AlertStates = new uint[]
                //    {
                //        responseData.Word(i),
                //        responseData.Word(i + 2),
                //        responseData.Word(i + 4),
                //        responseData.Word(i + 6),
                //        responseData.Word(i + 8),
                //        responseData.Word(i + 10),
                //        responseData.Word(i + 12),
                //        responseData.Word(i + 14),
                //    };
                //    result.AlertStates = alrs;
                //    break;
                case 0x02:
                    response.FaultResponse = new PodResponseFault();
                    response.StatusResponse = new PodResponseStatus();
                    response.Progress = (PodProgress) responseData.Byte(i++);

                    var deliveryStates = ParseDeliveryStates(responseData.Byte(i++));

                    response.StatusResponse.NotDelivered = responseData.Byte(i++);
                    response.StatusResponse.MessageSequence = responseData.Byte(i++);
                    response.StatusResponse.Delivered = responseData.Byte(i++);

                    response.FaultResponse.FaultCode = responseData.Byte(i++);
                    response.FaultResponse.FaultTimeMinutes = responseData.Word(i);

                    response.StatusResponse.Reservoir = responseData.Word(i + 2);
                    response.StatusResponse.ActiveMinutes = responseData.Word(i + 4);
                    i += 6;
                    response.StatusResponse.AlertMask = responseData.Byte(i++);
                    response.FaultResponse.TableAccessFault = responseData.Byte(i++);
                    var f17 = responseData.Byte(i++);
                    response.FaultResponse.InsulinStateTableCorr = (byte) (f17 >> 7);
                    response.FaultResponse.InternalFaultVars = (byte) ((f17 & 0x60) >> 6);
                    response.FaultResponse.FaultWhileBolus = (f17 & 0x10) > 0;
                    response.FaultResponse.ProgressBeforeFault = (PodProgress) (f17 & 0x0F);
                    var r18 = responseData.Byte(i++);

                    response.RadioResponse = new PodResponseRadio
                    {
                        PodLowGain = (byte) ((r18 & 0xC0) >> 6),
                        PodRssi = (byte) ((r18 & 0xC0) >> 6)
                    };

                    response.FaultResponse.ProgressBeforeFault2 = (PodProgress) (responseData.Byte(i++) & 0x0F);
                    // TODO: verify structure
                    // response.FaultResponse.FaultInformation2W = responseData.Byte(i++);
                    break;
                default:
                    throw new OmniCoreWorkflowException(FailureType.WorkflowPodResponseUnrecognized,
                        $"Failed to parse the information response of type {rt}");
            }
        }

        private (BolusState bolusState, BasalState basalState) ParseDeliveryStates(byte deliveryStates)
        {
            var bolusState = BolusState.Inactive;
            var basalState = BasalState.Suspended;

            if ((deliveryStates & 8) > 0)
                bolusState = BolusState.Extended;
            else if ((deliveryStates & 4) > 0)
                bolusState = BolusState.Immediate;

            if ((deliveryStates & 2) > 0)
                basalState = BasalState.Temporary;
            else if ((deliveryStates & 1) > 0)
                basalState = BasalState.Scheduled;

            return (bolusState, basalState);
        }
    }
}