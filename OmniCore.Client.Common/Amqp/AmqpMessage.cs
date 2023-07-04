using System.Text;

namespace OmniCore.Common.Amqp;

public class AmqpMessage
{
    public ulong Tag { get; set; }
    public bool Acknowledge { get; set; }
    public ulong Sequence { get; set; }
    public string? Exchange { get; set; }
    public string? Queue { get; set; }
    public string? Route { get; set; } = "";
    public string? UserId { get; set; }
    public byte[] Body { get; set; } = new byte[0];

    public string Text
    {
        get => Encoding.UTF8.GetString(Body);
        set => Body = Encoding.UTF8.GetBytes(value);
    }
}