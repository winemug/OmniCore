using Newtonsoft.Json;
using OmniCore.Model.Interfaces;
using OmniCore.Model.Utilities;
using SQLite;
using System;
using System.Collections.Generic;
using System.Text;

namespace OmniCore.Model.Eros
{
    public class ErosPodAlertStates : IPodAlertStates
    {
        [PrimaryKey, AutoIncrement]
        public uint? Id { get; set; }

        public DateTime Created { get; set; }
        public Guid PodId { get; set; }
        public uint AlertW278 { get; set; }
        [Ignore]
        public uint[] AlertStates { get; set; }

        public ErosPodAlertStates()
        {
            Created = DateTime.UtcNow;
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
