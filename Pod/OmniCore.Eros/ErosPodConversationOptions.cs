using OmniCore.Model.Enumerations;

namespace OmniCore.Eros
{
    public class ErosPodConversationOptions
    {
        public bool AllowAddressOverride { get; set; }
        public TransmissionPower? TransmissionPowerOverride { get; set; }
        public bool DynamicTxAttenuation { get; set; }
    }
}