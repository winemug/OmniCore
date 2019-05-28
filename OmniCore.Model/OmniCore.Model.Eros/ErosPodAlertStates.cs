using OmniCore.Model.Interfaces;
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
        public ushort? AlertW278 { get; set; }
        public ushort[] AlertStates { get; set; }
    }
}
