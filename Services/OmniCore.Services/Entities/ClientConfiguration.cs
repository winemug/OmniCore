using System;

namespace OmniCore.Services.Entities
{
    public enum CgmReceiverType
    {
        XdripWebService,
        NightscoutWebService,
        DexcomWebService,
    }
    
    public class ClientConfiguration
    {
        public Guid? AccountId { get; set; }
        public Guid? ClientId { get; set; }
        public string ClientAuthorizationToken { get; set; }
        public string Name { get; set; }
        public string Platform { get; set; }
        public string SoftwareVersion { get; set; }
        public string HardwareVersion { get; set; }
        public string OsVersion { get; set; }
        public Guid? DefaultProfileId { get; set; }
        public CgmReceiverType? ReceiverType { get; set; }
        public Guid? ReceiverProfileId { get; set; }
    }
}
