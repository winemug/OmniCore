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
        public string StringValue
        {
            get
            {
                return JsonConvert.SerializeObject(this);
            }
            set
            {
                JsonConvert.PopulateObject(value, this);
            }
        }
        [JsonIgnore]
        public string Identifier => RegistrationConstants.OmnipodEros;
        
    }
}