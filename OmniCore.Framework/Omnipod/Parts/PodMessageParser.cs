using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;
using OmniCore.Common.Pod;
using OmniCore.Framework.Omnipod.Parts;
using OmniCore.Services.Interfaces.Entities;
using OmniCore.Services.Interfaces.Pod;

namespace OmniCore.Services;

public static class PodMessageParser
{
    public static void yadayada()
    {
        var bm = new BeepMessageData
        {
            BeepNow = BeepType.Beeeeeep
        };

        var m = BuildMessage(bm);

        var data = new Bytes();
        var md = GetMessageData(data);

    }

    private static IMessage<T>? BuildMessage<T>(T messageData) where T : IRequestMessageData
    {
        return null;
    }

    private static IRequestMessageData? GetMessageData(Bytes data)
    {
        return null;
    }
}