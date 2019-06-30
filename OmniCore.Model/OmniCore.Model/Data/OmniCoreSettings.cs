using SQLite;
using System;
using System.Collections.Generic;
using System.Text;

namespace OmniCore.Model.Data
{
    public class OmniCoreSettings
    {
        [AutoIncrement, PrimaryKey]
        public int Id { get; set; }
        public bool AcceptCommandsFromAAPS { get; set; }
    }
}
