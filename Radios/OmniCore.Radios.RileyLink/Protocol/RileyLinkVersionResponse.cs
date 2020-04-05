using System.Text;
using OmniCore.Radios.RileyLink.Enumerations;

namespace OmniCore.Radios.RileyLink.Protocol
{
    public class RileyLinkVersionResponse : RileyLinkDefaultResponse
    {
        public string VersionString { get; private set; }

        protected override void ParseInternal(byte[] responseData)
        {
            VersionString = Encoding.ASCII.GetString(responseData);
        }
    }
}