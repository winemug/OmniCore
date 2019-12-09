using System;
using System.Collections.Generic;
using System.Text;
using OmniCore.Model.Interfaces.Entities;
using SQLite;

namespace OmniCore.Repository.Sqlite.Entities
{
    public class RadioEntity : Entity, IRadioEntity
    {
        public Guid DeviceUuid { get; set; }

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
    }
}
