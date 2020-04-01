using System;
using System.Collections.Generic;
using Innofactor.EfCoreJsonValueConverter;

namespace OmniCore.Model.Entities
{
    public class RadioEntity : Entity
    {
        public Guid DeviceUuid { get; set; }
        public Guid ServiceUuid { get; set; }
        public string DeviceName { get; set; }
        public string UserDescription { get; set; }

        [JsonField]
        public RadioOptions Options { get; set; } = new RadioOptions();

        public ICollection<PodRadioEntity> PodRadios { get; set; }
    }
}
