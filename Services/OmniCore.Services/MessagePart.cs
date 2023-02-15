using System.Text.Json;
using OmniCore.Services.Interfaces;

namespace OmniCore.Services;

public abstract class MessagePart
{
    public abstract bool RequiresNonce { get; }
    public abstract RadioMessageType Type { get; }
    public uint Nonce { get; set; }
    public Bytes Data { get; set; }
    public override string ToString()
    {
        return JsonSerializer.Serialize(this);
    }
}