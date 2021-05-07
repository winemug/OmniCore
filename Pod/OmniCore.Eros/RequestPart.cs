using OmniCore.Model.Enumerations;
using OmniCore.Model.Utilities;

namespace OmniCore.Eros
{
    public class RequestPart
    {
        public PartType PartType { get; set; }

        public Bytes PartData { get; set; }

        public bool RequiresNonce { get; set; }
    }
}