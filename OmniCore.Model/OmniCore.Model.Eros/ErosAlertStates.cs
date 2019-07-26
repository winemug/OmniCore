using System;
using Newtonsoft.Json;
using OmniCore.Model.Interfaces;
using SQLite;

namespace OmniCore.Model.Eros
{
    public class ErosAlertStates : IAlertStates
    {
        [PrimaryKey, AutoIncrement]
        public long? Id { get; set; }
        public Guid PodId { get; set; }
        public DateTimeOffset Created { get; set; }

        public long ResultId { get; set; }

        public uint AlertW278 { get; set; }
        [Ignore]
        public uint[] AlertStates { get; set; }

        public ErosAlertStates()
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
