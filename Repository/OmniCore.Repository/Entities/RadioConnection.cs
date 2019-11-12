using OmniCore.Repository.Enums;
using SQLite;
using System;
using System.Collections.Generic;
using System.Text;

namespace OmniCore.Repository.Entities
{
    public class RadioConnection : Entity
    {
        [Indexed]
        public long RadioId { get; set; }
        public long? PodId { get; set; }
        public long? RequestId { get; set; }
        public RadioConnectionEvent EventType { get; set; }
        public bool Successful { get; set; }
        public string ErrorText { get; set; }
        public byte[] Command { get; set; }
        public byte[] Response { get; set; }
    }
}
