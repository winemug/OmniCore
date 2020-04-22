using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using OmniCore.Model.Entities;
using OmniCore.Model.Enumerations;
using OmniCore.Model.Exceptions;
using OmniCore.Model.Interfaces.Services.Internal;
using OmniCore.Model.Utilities;
using OmniCore.Services;

namespace OmniCore.Eros
{
    public class ErosPodResponse : IErosPodResponse
    {
        public PodProgress? Progress { get; private set; }
        public bool? Faulted { get; private set; }
        public uint RadioAddress { get; private set; }
        public PodResponseFault FaultResponse { get; private set; }
        public PodResponseRadio RadioResponse { get; private set; }
        public PodResponseStatus StatusResponse { get; private set; }
        public PodResponseVersion VersionResponse { get; private set; }
        public bool IsValid { get; private set; }

        public ErosPodResponse()
        {
            IsValid = false;
        }

        public void ParseResponse(byte[] responseData)
        {
            var responseBytes = new Bytes(responseData);

            RadioAddress = responseBytes.DWord(0);

            var responseParts = new List<(byte, Bytes)>();
            var idx = 6;
            while (idx < responseBytes.Length - 2)
            {
                var responsePartCode = responseBytes[idx];
                var responsePartLength = responseBytes[idx + 1];
                var responseresponseData = responseBytes.Sub(idx + 2, idx + 2 + responsePartLength);
                responseParts.Add((responsePartCode, responseresponseData));
            }

            foreach (var (partCode, data) in responseParts)
                switch ((PartType) partCode)
                {
                    case PartType.ResponseVersionInfo:
                        ParseVersionResponse(data);
                        break;
                    case PartType.ResponseDetailInfoResponse:
                        ParseInformationResponse(data);
                        break;
                    case PartType.ResponseResyncResponse:
                        //parse_resync_response(pod as ErosPod, result);
                        break;
                    case PartType.ResponseStatus:
                        ParseStatusResponse(data);
                        break;
                    default:
                        IsValid = false;
                        throw new OmniCoreWorkflowException(FailureType.WorkflowPodResponseUnrecognized,
                            $"Unknown response type {partCode}");
                }

            // using var context = await RepositoryService.GetContextReadWrite(CancellationToken.None);
            // await context.PodResponses.AddAsync(response);
            // await context.Save(CancellationToken.None);
            //return response;
        }

        private void ParseVersionResponse(Bytes responseData)
        {
            VersionResponse = new PodResponseVersion();

            var lengthyResponse = false;
            var i = 0;
            if (responseData.Length == 27)
            {
                VersionResponse.VersionUnk2b = responseData.ByteBuffer[new Range(i, i + 7)];
                i += 7;
                lengthyResponse = true;
            }

            var mx = responseData.Byte(i++);
            var my = responseData.Byte(i++);
            var mz = responseData.Byte(i++);
            VersionResponse.VersionPm = $"{mx}.{my}.{mz}";

            var ix = responseData.Byte(i++);
            var iy = responseData.Byte(i++);
            var iz = responseData.Byte(i++);
            VersionResponse.VersionPi = $"{ix}.{iy}.{iz}";

            i++;

            Progress = (PodProgress) (responseData.Byte(i++) & 0x0F);


            VersionResponse.Lot = responseData.DWord(i);
            VersionResponse.Serial = responseData.DWord(i + 4);
            i += 8;
            if (!lengthyResponse)
            {
                var rb = responseData.Byte(i++);
                RadioResponse = new PodResponseRadio
                {
                    PodLowGain = (byte) (rb >> 6),
                    PodRssi = (byte) (rb & 0b00111111)
                };
            }

            VersionResponse.RadioAddress = responseData.DWord(i);
        }

        private void ParseStatusResponse(Bytes responseData)
        {
            StatusResponse = new PodResponseStatus();
            var s0 = responseData[0];
            var s1 = responseData.DWord(1);
            var s2 = responseData.DWord(5);

            var deliveryStates = ParseDeliveryStates((byte) (s0 >> 4));
            StatusResponse.BolusState = deliveryStates.bolusState;
            StatusResponse.BasalState = deliveryStates.basalState;
            Progress = (PodProgress) (s0 & 0xF);

            StatusResponse.MessageSequence = (int) (s1 & 0x00007800) >> 11;
            StatusResponse.Delivered = (int) (s1 & 0x0FFF8000) >> 15;
            StatusResponse.NotDelivered = (int) s1 & 0x000007FF;
            StatusResponse.Faulted = s2 >> 31 != 0;
            StatusResponse.AlertMask = (byte) ((s2 >> 23) & 0xFF);
            StatusResponse.ActiveMinutes = (int) (s2 & 0x007FFC00) >> 10;
            StatusResponse.Reservoir = (int) s2 & 0x000003FF;
        }

        private void ParseInformationResponse(Bytes responseData)
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
                    FaultResponse = new PodResponseFault();
                    StatusResponse = new PodResponseStatus();
                    Progress = (PodProgress) responseData.Byte(i++);

                    var deliveryStates = ParseDeliveryStates(responseData.Byte(i++));
                    StatusResponse.BolusState = deliveryStates.bolusState;
                    StatusResponse.BasalState = deliveryStates.basalState;
                    StatusResponse.NotDelivered = responseData.Byte(i++);
                    StatusResponse.MessageSequence = responseData.Byte(i++);
                    StatusResponse.Delivered = responseData.Byte(i++);

                    FaultResponse.FaultCode = responseData.Byte(i++);
                    FaultResponse.FaultTimeMinutes = responseData.Word(i);

                    StatusResponse.Reservoir = responseData.Word(i + 2);
                    StatusResponse.ActiveMinutes = responseData.Word(i + 4);
                    i += 6;
                    StatusResponse.AlertMask = responseData.Byte(i++);
                    FaultResponse.TableAccessFault = responseData.Byte(i++);
                    var f17 = responseData.Byte(i++);
                    FaultResponse.InsulinStateTableCorr = (byte) (f17 >> 7);
                    FaultResponse.InternalFaultVars = (byte) ((f17 & 0x60) >> 6);
                    FaultResponse.FaultWhileBolus = (f17 & 0x10) > 0;
                    FaultResponse.ProgressBeforeFault = (PodProgress) (f17 & 0x0F);
                    var r18 = responseData.Byte(i++);

                    RadioResponse = new PodResponseRadio
                    {
                        PodLowGain = (byte) ((r18 & 0xC0) >> 6),
                        PodRssi = (byte) ((r18 & 0xC0) >> 6)
                    };

                    FaultResponse.ProgressBeforeFault2 = (PodProgress) (responseData.Byte(i++) & 0x0F);
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