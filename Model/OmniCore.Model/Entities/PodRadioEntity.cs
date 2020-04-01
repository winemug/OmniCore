using System;
using System.Collections.Generic;
using System.Text;

namespace OmniCore.Model.Entities
{
    public class PodRadioEntity : Entity
    {
        public PodEntity Pod { get; set; }
        public RadioEntity Radio { get; set; }
    }
}
