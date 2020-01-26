using System;
using System.Collections.Generic;
using System.Text;
using OmniCore.Model.Enumerations;

namespace OmniCore.Model.Entities
{
    public class RadioEventEntity : Entity
    {
        public RadioEntity Radio { get; set; }

        public PodEntity Pod { get; set; }
        public PodRequestEntity Request { get; set; }
        public RadioEvent EventType { get; set; }
        public byte[] Data { get; set; }
        public string Text { get; set; }
        public int? Rssi { get; set; }
    }
}
