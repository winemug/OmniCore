using System.Text;

namespace OmniCore.Services.Interfaces;

public class AmqpMessage
{
    public byte[] Body { get; set; }

    public string Id { get; set; }

    public string Text
    {
        get => Encoding.UTF8.GetString(Body);
        set => Body = Encoding.UTF8.GetBytes(value);
    }
}