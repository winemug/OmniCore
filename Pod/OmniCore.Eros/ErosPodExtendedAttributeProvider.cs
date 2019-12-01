using OmniCore.Model.Constants;
using OmniCore.Model.Interfaces.Entities;

namespace OmniCore.Eros
{
    public class ErosPodExtendedAttributeProvider : IExtendedAttributeProvider
    {
        public string Identifier => RegistrationConstants.OmnipodEros;
        public IExtendedAttribute New(string serializedValue = null)
        {
            var attr = new ErosPodExtendedAttribute();
            if (serializedValue != null)
                attr.ExtensionValue = serializedValue;
            return attr;
        }
    }
}