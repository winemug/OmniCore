using SQLite;
using System;
using System.Collections.Generic;
using System.Text;

namespace OmniCore.Repository.Entities
{
    public class RadioConnectionLog : Entity
    {
        [Indexed]
        public long RadioId { get; set; }
        public long PodId { get; set; }
        public bool ConnectEvent { get; set; }
        public bool DisconnectEvent { get; set; }
        public bool CommandEvent { get; set; }
        public bool Successful { get; set; }
        public string ErrorText { get; set; }
        public byte[] Command { get; set; }
        public byte[] Response { get; set; }
        public int? Rssi { get; set; }
    }
}
