using Newtonsoft.Json;
using SQLite;
using System;
using System.Collections.Generic;
using System.Text;

namespace OmniCore.Model.Eros.Data
{
    public class ErosRadioPreferences
    {
        [Ignore]
        public Guid[] PreferredRadios { get; set; }

        public string PreferredRadiosJson
        {
            get
            {
                return JsonConvert.SerializeObject(PreferredRadios ?? new Guid[0]);
            }
            set
            {
                PreferredRadios = JsonConvert.DeserializeObject<Guid[]>(value);
            }
        }

        public bool ConnectToAny { get; set; }
    }
}
