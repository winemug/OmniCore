using OmniCore.Radios.RileyLink.Enumerations;

namespace OmniCore.Radios.RileyLink.Protocol
{
    public class RileyLinkRegisterValueResponse : RileyLinkResponse
    {
        public byte Value { get; private set; }

        protected override void ParseResponse(byte[] responseData)
        {
            Value = responseData[0];
        }
    }
}