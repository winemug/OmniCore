using System;
using OmniCore.Model.Interfaces.Platform.Common.Data.Entities;
using OmniCore.Repository.Sqlite.Entities;

namespace OmniCore.Repository.Sqlite.Entities
{
    public class MigrationHistoryEntity : Entity, IMigrationHistoryEntity
    {
        public int? FromMajor { get; set; }
        public int? FromMinor { get; set; }
        public int? FromBuild { get; set; }
        public int? FromRevision { get; set; }

        public int ToMajor { get; set; }
        public int ToMinor { get; set; }
        public int ToBuild { get; set; }
        public int ToRevision { get; set; }

        public string ImportPath { get; set; }
    }
}