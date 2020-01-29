using Innofactor.EfCoreJsonValueConverter;
using OmniCore.Model.Enumerations;

namespace OmniCore.Model.Entities
{
    public class PodResponseEntity : Entity
    {
        public PodRequestEntity PodRequest { get; set; }
        public PodProgress? Progress { get; set; }
        public bool? Faulted { get; set; }

        [JsonField]
        public PodResponseFault FaultResponse { get; set; }
        [JsonField]
        public PodResponseRadio RadioResponse { get; set; }
        [JsonField]
        public PodResponseStatus StatusResponse { get; set; }
        [JsonField]
        public PodResponseVersion VersionResponse { get; set; }
    }
}