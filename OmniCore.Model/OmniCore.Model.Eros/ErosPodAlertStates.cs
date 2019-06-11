using Newtonsoft.Json;
using OmniCore.Model.Interfaces;
using OmniCore.Model.Utilities;
using SQLite;
using SQLiteNetExtensions.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace OmniCore.Model.Eros
{
    [Table("AlertState")]
    public class ErosPodAlertStates : IPodAlertStates
    {
        [PrimaryKey, AutoIncrement]
        public long? Id { get; set; }
        public Guid PodId { get; set; }
        public DateTime Created { get; set; }

        [ForeignKey(typeof(MessageExchangeResult))]
        public long ResultId { get; set; }

        public uint AlertW278 { get; set; }
        [Ignore]
        public uint[] AlertStates { get; set; }

        public ErosPodAlertStates()
        {
        }

        public string AlertStatesJson
        {
            get
            {
                return JsonConvert.SerializeObject(AlertStates);
            }
            set
            {
                AlertStates = JsonConvert.DeserializeObject<uint[]>(value);
            }
        }
    }
}
