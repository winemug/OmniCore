using System;
using System.Net.NetworkInformation;

namespace OmniCore.Services
{
    public class BleDevice
    {
        public string DeviceName { get; set; }
        public Guid Id { get; set; }
    }
}