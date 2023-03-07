using System;
using OmniCore.Services.Interfaces;

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
            HardwareVersionMajor = data[0];
            HardwareVersionMinor = data[1];
            HardwareVersionRevision = data[2];

            FirmwareVersionMajor = data[3];
            FirmwareVersionMinor = data[4];
            FirmwareVersionRevision = data[5];
            ProductId = data[6];
            Progress = (PodProgress)data[7];
            Lot = data.DWord(8);
            Serial = data.DWord(12);
            RadioLowGain = (data[16] >> 6) & 0b00000011;
            Rssi = data[16] & 0b00111111;
            AssignedAddress = data.DWord(17);
        }

        if (data.Length == 27)
        {
            PulseVolumeMicroUnits = data.Word(0);
            PulseRatePer125ms = data[2];
            PrimingPulseRatePer125ms = data[3];
            PrimingPulseCount = data[4];
            CannulaInsertPulseCount = data[5];
            MaximumLifeTimeHours = data[6];

            HardwareVersionMajor = data[7];
            HardwareVersionMinor = data[8];
            HardwareVersionRevision = data[9];

            FirmwareVersionMajor = data[10];
            FirmwareVersionMinor = data[11];
            FirmwareVersionRevision = data[12];

            ProductId = data[13];
            Progress = (PodProgress)data[14];
            Lot = data.DWord(15);
            Serial = data.DWord(19);
            AssignedAddress = data.DWord(23);
        }
    }

    public override bool RequiresNonce => false;
    public override PodMessageType Type => PodMessageType.ResponseVersionInfo;

    public int HardwareVersionMajor { get; }
    public int HardwareVersionMinor { get; }
    public int HardwareVersionRevision { get; }

    public int FirmwareVersionMajor { get; }
    public int FirmwareVersionMinor { get; }
    public int FirmwareVersionRevision { get; }

    public int ProductId { get; }
    public PodProgress Progress { get; }
    public uint Lot { get; }
    public uint Serial { get; }

    public uint AssignedAddress { get; }
    public int? RadioLowGain { get; }
    public int? Rssi { get; }

    public int? PulseVolumeMicroUnits { get; }
    public int? PulseRatePer125ms { get; }
    public int? PrimingPulseRatePer125ms { get; }
    public int? PrimingPulseCount { get; }
    public int? CannulaInsertPulseCount { get; }
    public int? MaximumLifeTimeHours { get; }
}