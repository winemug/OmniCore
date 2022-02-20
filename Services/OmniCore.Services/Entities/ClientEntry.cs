using System;

namespace OmniCore.Services.Interfaces
{
    public class ClientEntry
    {
        public Guid? Id { get; set; }
        public string Name { get; set; }
        public string SoftwareVersion { get; set; }
        public string HardwareVersion { get; set; }
        public string Platfrom { get; set; }
    }
}