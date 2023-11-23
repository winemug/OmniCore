using OmniCore.Shared.Entities.Omnipod.Parts;
using OmniCore.Shared.Enums;
using OmniCore.Shared.Extensions;

namespace OmniCore.Framework.Omnipod.Parts;

public class RequestBlockSchedule : IMessagePart
{
    public required byte TableIndex { get; set; }
    public required int LeadBlockDuration125Milliseconds { get; set; }
    public required int LeadBlockPulseCount { get; set; }
    public byte? CurrentExpandedBlockIndex { get; set; }
    public required int[] PulseCountList { get; set; }

    public int ToBytes(Span<byte> span)
    {
        span[0] = TableIndex;
        if (CurrentExpandedBlockIndex != null)
            span[3] = CurrentExpandedBlockIndex.Value;
        else
            span[3] = (byte)PulseCountList.Length;

        span[4..].Write16((UInt16)LeadBlockDuration125Milliseconds);
        span[6..].Write16((UInt16)LeadBlockPulseCount);

        var len = PackPulsesIntoPulseBlockSchedule(PulseCountList, span[8..]) + 8;

        int checksum = span[3] + span[4] + span[5] + span[6] + span[7];
        foreach (var pulseCount in PulseCountList)
            checksum += pulseCount;

        span[1..].Write16((UInt16)checksum);
        return len;
    }

    public static IMessagePart ToInstance(Span<byte> span)
    {
        return new RequestBlockSchedule
        {
            TableIndex = span[0],
            CurrentExpandedBlockIndex = span[3],
            LeadBlockDuration125Milliseconds = span[4..].Read16(),
            LeadBlockPulseCount = span[6..].Read16(),
            PulseCountList = GetPulseCountList(span[8..])
        };
    }

    private static int[] GetPulseCountList(Span<byte> buffer)
    {
        List<int> pulseCounts = new List<int>();
        int idx = 0;
        while (idx < buffer.Length)
        {
            var blockCount = buffer[idx..].ReadBits(0, 4) + 1;
            var addExtraPulse = buffer[idx..].ReadBits(4, 1) == 1;
            var pulsesPerBlock = (int) buffer[idx..].ReadBits(5, 11);

            for (var i = 0; i<blockCount; i++)
                pulseCounts.Add( (addExtraPulse && (i % 2 == 1) ? 1 : 0) + pulsesPerBlock );
            idx += 2;
        }
        return pulseCounts.ToArray();
    }

    private int PackPulsesIntoPulseBlockSchedule(int[] pulses, Span<byte> buffer)
    {
        int bufferIndex = 0;
        int pulseBlockPulses = pulses[0];
        int currentBlockCount = 1;
        bool pulseBlockExtraPulse = false;
        for(int i = 1; i < pulses.Length; i++)
        {
            if (currentBlockCount == 1)
                pulseBlockExtraPulse = pulses[i] == pulseBlockPulses + 1;

            if (pulses[i] != (pulseBlockExtraPulse && (currentBlockCount % 2 == 1) ? pulseBlockPulses + 1 : pulseBlockPulses))
            {
                AddPackedPulseBlock(currentBlockCount, pulseBlockPulses, pulseBlockExtraPulse, buffer[bufferIndex..]);
                bufferIndex += 2;
                currentBlockCount = 1;
                pulseBlockPulses = pulses[i];
            }
            else
                currentBlockCount++;
        }
        AddPackedPulseBlock(currentBlockCount, pulseBlockPulses, pulseBlockExtraPulse, buffer[bufferIndex..]);
        bufferIndex += 2;
        return bufferIndex;
    }

    private void AddPackedPulseBlock(int blockCount, int pulsesPerBlock, bool addExtraPulse, Span<byte> buffer)
    {
        buffer.WriteBits(blockCount - 1, 0, 4);
        buffer.WriteBits((addExtraPulse ? 1:0), 4, 1);
        buffer.WriteBits(pulsesPerBlock, 5, 11);
    }
}