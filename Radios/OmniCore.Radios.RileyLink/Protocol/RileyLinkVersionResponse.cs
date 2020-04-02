using System.Text;
using OmniCore.Radios.RileyLink.Enumerations;

namespace OmniCore.Radios.RileyLink.Protocol
{
    public class RileyLinkVersionResponse : RileyLinkResponse
    {
        public string VersionString { get; private set; }

        protected override void ParseResponse(byte[] responseData)
        {
            VersionString = Encoding.ASCII.GetString(responseData);
        }
    }
}