using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using OmniCore.Model.Interfaces.Services;
using OmniCore.Model.Interfaces.Services.Internal;

namespace OmniCore.Services.Integration
{
    public class MqttIntegration : IIntegrationComponent
    {
        public string ComponentName
        {
            get => "MqttIntegration";
        }
        public string ComponentDescription
        {
            get => "Provides integration with mqtt servers";
        }
        public bool ComponentEnabled { get; set; }
        public Task InitializeComponent(ICoreService parentService)
        {
            throw new NotImplementedException();
        }
        public void Dispose()
        {
        }

    }
}
