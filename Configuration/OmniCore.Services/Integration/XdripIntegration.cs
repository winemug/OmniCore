using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using OmniCore.Model.Interfaces.Services;
using OmniCore.Model.Interfaces.Services.Internal;

namespace OmniCore.Services.Integration
{
    public class XdripIntegration : IIntegrationComponent
    {
        public string ComponentName => "Xdrip Local";
        public string ComponentDescription => "Registers blood glucose metrics received from the Xdrip application installed on the same device.";
        public bool ComponentEnabled { get; set; }

        private ICoreService ParentService;

        public Task InitializeComponent(ICoreService parentService)
        {
            ParentService = parentService;
            return Task.CompletedTask;
        }

        public void Dispose()
        {
        }
    }
}
