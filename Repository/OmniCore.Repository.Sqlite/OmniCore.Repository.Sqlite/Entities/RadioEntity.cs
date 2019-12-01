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
        public string DeviceIdReadable { get; set; }
        public string DeviceName { get; set; }
        public string UserDescription { get; set; }
    }
}
