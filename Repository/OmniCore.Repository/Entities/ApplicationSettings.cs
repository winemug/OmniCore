using SQLite;
using System;
using System.Collections.Generic;
using System.Text;

namespace OmniCore.Repository.Entities
{
    public class ApplicationSettings : UpdateableEntity
    {
        public bool AcceptCommandsFromAAPS { get; set; }
    }
}