using SQLite;
using System;
using System.Collections.Generic;
using System.Text;

namespace OmniCore.Repository.Entities
{
    public class ApplicationSettings : UpdateableEntity
    {
        public bool RadioConnectionCheck { get; set; }

        public int? RadioConnectionCheckInterval { get; set; }

        public int? PodStatusUpdateInterval { get; set; }
    }
}