namespace OmniCore.Model.Interfaces.Entities
{
    public interface IExtendedAttribute
    {
        string ExtensionIdentifier { get; }
        string ExtensionValue { get; set; }
    }
}