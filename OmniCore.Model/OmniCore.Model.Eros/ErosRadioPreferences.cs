using System;
using Newtonsoft.Json;
using SQLite;

namespace OmniCore.Model.Eros
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
