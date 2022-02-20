using System;

namespace OmniCore.Services.Entities
{
    public class ClientConfiguration
    {
        public string Name { get; set; }
        public string Platform { get; set; }
        public string SoftwareVersion { get; set; }
        public string HardwareVersion { get; set; }
        public string OsVersion { get; set; }
        public Guid? AccountId { get; set; }
        public Guid? ClientId { get; set; }
        public string Token { get; set; }
    }
}
