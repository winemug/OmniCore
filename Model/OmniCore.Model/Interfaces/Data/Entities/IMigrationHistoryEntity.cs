namespace OmniCore.Model.Interfaces.Common.Data.Entities
{
    public interface IMigrationHistoryEntity : IEntity
    {
        int? FromMajor { get; set; }
        int? FromMinor { get; set; }
        int? FromBuild { get; set; }
        int? FromRevision { get; set; }

        int ToMajor { get; set; }
        int ToMinor { get; set; }
        int ToBuild { get; set; }
        int ToRevision { get; set; }

        string ImportPath { get; set; }
    }
}