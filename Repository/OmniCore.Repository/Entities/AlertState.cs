using Newtonsoft.Json;
using SQLite;
using System;

namespace OmniCore.Model.Entities
{
    public class AlertState : Entity
    {
        [Indexed]
        public long RequestId {get; set;}
        public uint AlertW278 { get; set; }

        [Ignore]
        public int[] AlertStates
        {
            get
            {
                if (String.IsNullOrEmpty(AlertStatesJson))
                    return null;
                else
                    return JsonConvert.DeserializeObject<int[]>(AlertStatesJson);
            }
            set
            {
                if (value == null)
                    AlertStatesJson = null;
                else
                    AlertStatesJson = JsonConvert.SerializeObject(value);
            }
        }
        public string AlertStatesJson { get; set; }
    }
}
