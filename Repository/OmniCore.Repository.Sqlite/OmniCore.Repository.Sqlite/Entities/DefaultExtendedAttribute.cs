using OmniCore.Model.Interfaces.Entities;

namespace OmniCore.Repository.Sqlite.Entities
{
    public class DefaultExtendedAttribute : IExtendedAttribute
    {
        public string ExtensionIdentifier { get; } = null;
        public string ExtensionValue { get; set; }
    }
}