using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using OmniCore.Model.Interfaces.Common.Data.Entities;
using SQLite;

namespace OmniCore.Repository.Sqlite.Entities
{
    public class RadioEntity : Entity, IRadioEntity
    {
        public Guid DeviceUuid { get; set; }

        [Ignore]
        public Guid[] ServiceUuids { get; set; }

        public string ServiceUuidsJoined
        {
            get
            {
                if (ServiceUuids == null)
                    return null;
                return string.Join(',', ServiceUuids);
            }
            set
            {
                if (value == null)
                    ServiceUuids = new Guid[0];
                else
                {
                    ServiceUuids = value.Split(',').AsEnumerable()
                        .Select(Guid.Parse).ToArray();
                }
            }
        }

        [Ignore]
        public string DeviceIdReadable
        {
            get
            {
                var gb = DeviceUuid.ToByteArray();
                return $"{gb[10]:X2}:{gb[11]:X2}:{gb[12]:X2}:{gb[13]:X2}:{gb[14]:X2}:{gb[15]:X2}";
            }
        }
        public string DeviceName { get; set; }
        public string UserDescription { get; set; }

        public bool KeepConnected { get; set; }
        public TimeSpan ResponseTimeout { get; set; }
        public TimeSpan ConnectTimeout { get; set; }

        public string ConfigurationJson { get; set; }
    }
}
