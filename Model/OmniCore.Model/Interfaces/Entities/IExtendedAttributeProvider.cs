using System.Threading.Tasks;

namespace OmniCore.Model.Interfaces.Entities
{
    public interface IExtendedAttributeProvider
    {
        string Identifier { get; }
        IExtendedAttribute New(string serializedValue = null);
    }
}