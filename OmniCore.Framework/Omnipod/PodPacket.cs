using System;
using System.Diagnostics;
using System.Linq;
using OmniCore.Services.Interfaces;
using OmniCore.Services.Interfaces.Entities;
using OmniCore.Services.Interfaces.Pod;
using OmniCore.Services.Interfaces.Radio;

namespace OmniCore.Services;

public class PodPacket : IPodPacket
{
    public PodPacket(uint address, PodPacketType type, int sequence, Bytes data)
    {
        Address = address;
        Type = type;
        Sequence = sequence;
        Data = data;
    }

    public int? Rssi { get; set; }
    public uint Address { get; set; }

    public PodPacketType Type { get; set; }

    public int Sequence { get; set; }

    public Bytes Data { get; set; }

    public Bytes ToRadioData()
    {
        var radioData = new Bytes(Address);

        var b4 = (byte)(Sequence & 0b00011111);
        switch (Type)
        {
            case PodPacketType.Ack:
                b4 |= 0b01000000;
                break;
            case PodPacketType.Con:
                b4 |= 0b10000000;
                break;
            case PodPacketType.Pdm:
                b4 |= 0b10100000;
                break;
            case PodPacketType.Pod:
                b4 |= 0b11100000;
                break;
            default:
                throw new ApplicationException("Unknown packet type");
        }

        radioData.Append(b4);
        if (Data != null && Data.Length > 0)
            radioData.Append(Data);

        radioData.Append(CrcUtil.Crc8(radioData));
        return ManchesterCodec.Encode(radioData, 40);
    }

    public static PodPacket? FromExchangeResult(BleExchangeResult result, uint? expectedAddress = default)
    {
        if (result.CommunicationResult != BleCommunicationResult.OK ||
            result.ResponseCode != RileyLinkResponse.CommandSuccess ||
            result.ResponseData == null ||
            result.ResponseData.Length < 2)
            return null;
        var rssi = ((sbyte)result.ResponseData[0] - 127) / 2; // -128 to 127

        var data = result.ResponseData.Sub(2);
        var pp = FromRadioData(data, rssi);
        if (expectedAddress.HasValue && pp?.Address != expectedAddress)
            return null;
        return pp;
    }
    
    public static PodPacket? FromRadioData(Bytes data, int radioRssi)
    {
        var decodedData = ManchesterCodec.Decode(data);
        if (decodedData.Length < 6)
            return null;
        // throw new ApplicationException("Invalid packet");

        var crcByte = decodedData[decodedData.Length - 1];
        var crc = CrcUtil.Crc8(decodedData.Sub(0, decodedData.Length - 1));

        if (crc != crcByte)
            return null;
        // throw new ApplicationException("Invalid crc");

        // Debug.WriteLine($"RAWI: {data.ToHexString()}");
        var address = decodedData.DWord(0);
        PodPacketType type;
        switch (decodedData[4] & 0b11100000)
        {
            case 0b01000000:
                type = PodPacketType.Ack;
                break;
            case 0b10000000:
                type = PodPacketType.Con;
                break;
            case 0b10100000:
                type = PodPacketType.Pdm;
                break;
            case 0b11100000:
                type = PodPacketType.Pod;
                break;
            default:
                return null;
            // throw new ApplicationException("Unknown packet type");
        }

        var sequence = decodedData[4] & 0b00011111;

        return new PodPacket(address, type, sequence, decodedData.Sub(5, decodedData.Length - 1))
        {
            Rssi = radioRssi
        };
    }

    public override string ToString()
    {
        return $"Address: {Address:X} Type: {Type} Seq: {Sequence} Data: {Data}" +
               (Rssi.HasValue ? $" RSSI: {Rssi}" : "");
    }
}