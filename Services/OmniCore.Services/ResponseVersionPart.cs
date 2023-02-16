using System;
using OmniCore.Services.Interfaces;

namespace OmniCore.Services;

public class ResponseVersionPart : MessagePart
{
    public override bool RequiresNonce => false;
    public override RadioMessageType Type => RadioMessageType.ResponseVersionInfo;

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

    public ResponseVersionPart(Bytes Data)
    {
        if (Data.Length != 21 && Data.Length != 27)
            throw new ApplicationException();

        if (Data.Length == 21)
        {
            HardwareVersionMajor = Data[0];
            HardwareVersionMinor = Data[1];
            HardwareVersionRevision = Data[2];

            FirmwareVersionMajor = Data[3];
            FirmwareVersionMinor = Data[4];
            FirmwareVersionRevision = Data[5];
            ProductId = Data[6];
            Progress = (PodProgress)Data[7];
            Lot = Data.DWord(8);
            Serial = Data.DWord(12);
            RadioLowGain = (Data[16] >> 6) & 0b00000011;
            Rssi = Data[16] & 0b00111111;
            AssignedAddress = Data.DWord(17);
        }

        if (Data.Length == 27)
        {
            PulseVolumeMicroUnits = Data.Word(0);
            PulseRatePer125ms = Data[2];
            PrimingPulseRatePer125ms = Data[3];
            PrimingPulseCount = Data[4];
            CannulaInsertPulseCount = Data[5];
            MaximumLifeTimeHours = Data[6];
            
            HardwareVersionMajor = Data[7];
            HardwareVersionMinor = Data[8];
            HardwareVersionRevision = Data[9];

            FirmwareVersionMajor = Data[10];
            FirmwareVersionMinor = Data[11];
            FirmwareVersionRevision = Data[12];

            ProductId = Data[13];
            Progress = (PodProgress)Data[14];
            Lot = Data.DWord(15);
            Serial = Data.DWord(19);
            AssignedAddress = Data.DWord(23);
        }
    }
}