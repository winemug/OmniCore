using System.Text.Json.Serialization;

namespace OmniCore.Common.Pod;

public interface IMessageData
{
    [JsonIgnore] static abstract Predicate<IMessageParts> CanParse { get; }

    IMessageParts ToParts();
    IMessageData FromParts(IMessageParts parts);
}