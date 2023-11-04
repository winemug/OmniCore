using OmniCore.Shared;
using OmniCore.Shared.Entities;
using OmniCore.Shared.Entities.Omnipod.Parts;
using OmniCore.Shared.Enums;
using OmniCore.Shared.Extensions;

namespace OmniCore.Framework.Omnipod.Parts;

public class RequestConfigureAlerts : IMessagePart
{
    public required AlertConfiguration[] AlertConfigurations { get; set; }
    public static IMessagePart ToInstance(Span<byte> span)
    {
        var idx = 0;
        var alertConfigurations = new List<AlertConfiguration>();
        while (idx < span.Length)
        {
            var ac = new AlertConfiguration
            {
                AlertIndex = (int)span[idx..].ReadBits(1, 3),
                SetActive = span[idx..].ReadBits(4, 1) > 0,
                ReservoirBased = span[idx..].ReadBits(5, 1) > 0,
                SetAutoOff = span[idx..].ReadBits(6, 1) > 0,
                AlertDurationMinutes = (int)span[idx..].ReadBits(7, 9),
                AlertAfter = (int)span[idx..].ReadBits(18, 14),
                BeepPattern = (BeepPattern)span[idx..].ReadBits(36, 4),
                BeepType = (BeepType)span[idx..].ReadBits(44, 4)
            };
            alertConfigurations.Add(ac);
            idx += 6;
        }

        return new RequestConfigureAlerts
        {
            AlertConfigurations = alertConfigurations.ToArray()
        };
    }

    public int ToBytes(Span<byte> span)
    {
        if (!AlertConfigurations.Any())
            throw new ApplicationException("Need to have at least one configuration");

        if (AlertConfigurations.DistinctBy(x => x.AlertIndex).Count() !=
            AlertConfigurations.Count())
            throw new ApplicationException("Alert indices need to be unique");

        int idx = 0;
        foreach (var ac in AlertConfigurations)
        {
            span[idx..].WriteBits(ac.AlertIndex, 1, 3);
            span[idx..].WriteBit(ac.SetActive, 4);
            span[idx..].WriteBit(ac.ReservoirBased, 5);
            span[idx..].WriteBit(ac.SetAutoOff, 6);
            span[idx..].WriteBits(ac.AlertDurationMinutes, 7, 9);
            span[idx..].WriteBits(ac.AlertAfter, 18, 14);
            span[idx..].WriteBits((uint)ac.BeepPattern, 36, 4);
            span[idx..].WriteBits((uint)ac.BeepPattern, 44, 4);
            idx += 6;
        }

        return idx;
    }
}