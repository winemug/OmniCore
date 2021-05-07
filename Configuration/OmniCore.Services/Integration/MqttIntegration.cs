﻿using System;
using System.Threading.Tasks;
using OmniCore.Model.Interfaces.Services;
using OmniCore.Model.Interfaces.Services.Internal;

namespace OmniCore.Services.Integration
{
    public class MqttIntegration : IIntegrationComponent
    {
        public string ComponentName => "MqttIntegration";

        public string ComponentDescription => "Provides integration with mqtt servers";

        public bool ComponentEnabled { get; set; }

        public Task InitializeComponent(IService parentService)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
        }
    }
}