namespace OmniCore.Radios.RileyLink.Protocol
{
    public class RileyLinkRegisterValueResponse : RileyLinkStandardResponse
    {
        public byte Value { get; private set; }

        protected override void ParseInternal(byte[] responseData)
        {
            Value = responseData[0];
        }
    }
}