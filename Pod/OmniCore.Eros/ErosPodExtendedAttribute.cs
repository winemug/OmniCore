using System;
using Newtonsoft.Json;
using OmniCore.Model.Constants;
using OmniCore.Model.Interfaces.Entities;
using OmniCore.Model.Interfaces.Repositories;
using OmniCore.Model.Interfaces.Services;
using OmniCore.Model.Interfaces.Workflow;

namespace OmniCore.Eros
{
    public class ErosPodExtendedAttribute : IExtendedAttribute
    {
        public uint RadioAddress { get; set; }
        
        [JsonIgnore]
        public string ExtensionIdentifier => RegistrationConstants.OmnipodEros;

        private string ExtensionValueCached = null;
        [JsonIgnore]
        public string ExtensionValue
        {
            get
            {
                if (ExtensionValueCached == null)
                {
                    ExtensionValueCached = JsonConvert.SerializeObject(this);
                }

                return ExtensionValueCached;
            }
            set
            {
                if (value != ExtensionValueCached)
                {
                    JsonConvert.PopulateObject(value, this);
                    ExtensionValueCached = value;
                }
            }
        }
    }
}