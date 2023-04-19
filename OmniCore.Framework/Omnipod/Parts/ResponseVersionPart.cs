using System;
using OmniCore.Common.Pod;
using OmniCore.Services.Interfaces;
using OmniCore.Services.Interfaces.Entities;
using OmniCore.Services.Interfaces.Pod;

namespace OmniCore.Services;

public class ResponseVersionPart : MessagePart
{
    public ResponseVersionPart(Bytes data)
    {
        Data = data;
        if (data.Length != 21 && data.Length != 27)
            throw new ApplicationException();

        if (data.Length == 21)
        {
            VersionModel = new PodVersionModel
            {
                HardwareVersionMajor = data[0],
                HardwareVersionMinor = data[1],
                HardwareVersionRevision = data[2],

                FirmwareVersionMajor = data[3],
                FirmwareVersionMinor = data[4],
                FirmwareVersionRevision = data[5],
                ProductId = data[6],
                Lot = data.DWord(8),
                Serial = data.DWord(12),
                AssignedAddress = data.DWord(17),
            };

            RadioMeasurementsModel = new PodRadioMeasurementsModel
            {
                RadioLowGain = (data[16] >> 6) & 0b00000011,
                Rssi = data[16] & 0b00111111,
            };
            ProgressModel = new PodProgressModel
            {
                Progress = (PodProgress)data[7],
                Faulted = data[14] > 9 && data[7] < 15
            };
        }

        if (data.Length == 27)
        {
            ActivationParametersModel = new PodActivationParametersModel
            {
                PulseVolumeMicroUnits = data.Word(0),
                PulseRatePer125ms = data[2],
                PrimingPulseRatePer125ms = data[3],
                PrimingPulseCount = data[4],
                CannulaInsertPulseCount = data[5],
                MaximumLifeTimeHours = data[6],
            };

            VersionModel = new PodVersionModel
            {
                HardwareVersionMajor = data[7],
                HardwareVersionMinor = data[8],
                HardwareVersionRevision = data[9],

                FirmwareVersionMajor = data[10],
                FirmwareVersionMinor = data[11],
                FirmwareVersionRevision = data[12],

                ProductId = data[13],
                Lot = data.DWord(15),
                Serial = data.DWord(19),
                AssignedAddress = data.DWord(23),
            };
            ProgressModel = new PodProgressModel
            {
                Progress = (PodProgress)data[14],
                Faulted = data[14] > 9 && data[14] < 15
            };
        }
    }
    
    public PodVersionModel? VersionModel { get; set; }
    public PodRadioMeasurementsModel? RadioMeasurementsModel { get; set; }
    public PodActivationParametersModel? ActivationParametersModel { get; set; }
    public PodProgressModel ProgressModel { get; set; }

    public override bool RequiresNonce => false;

    public override PodMessagePartType Type => PodMessagePartType.ResponseVersionInfo;
}