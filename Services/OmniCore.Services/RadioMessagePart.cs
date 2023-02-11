using OmniCore.Services.Interfaces;
using Xamarin.Forms;

namespace OmniCore.Services
{
    public abstract class RadioMessagePart
    {
        public abstract RadioMessageType Type { get; }
        public uint? Nonce { get; set; }
        public Bytes Data { get; set; }
    }
}