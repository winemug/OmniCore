using System.Diagnostics;
using Dapper;
using OmniCore.Services;
using OmniCore.Services.Interfaces;
using OmniCore.Services.Interfaces.Amqp;
using OmniCore.Services.Interfaces.Entities;
using OmniCore.Services.Interfaces.Platform;

namespace OmniCore.Maui.ViewModels
{
    public class TestViewModel : BaseViewModel
    {
        // private readonly IForegroundServiceHelper _foregroundServiceHelper;
        // private IPlatformInfo _platformInfo;

        private int dac;

        public TestViewModel()
        {
        }
    }
}