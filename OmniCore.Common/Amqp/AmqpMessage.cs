using System.Text;

namespace OmniCore.Services.Interfaces.Amqp;

public class AmqpMessage
{
    public string? Type { get; set; }
    public string Route { get; set; } = "";
    public byte[] Body { get; set; } = new byte[0];

    public string Text
    {
        get => Encoding.UTF8.GetString(Body);
        set => Body = Encoding.UTF8.GetBytes(value);
    }
    public Task OnPublishConfirmed { get; set; }
}