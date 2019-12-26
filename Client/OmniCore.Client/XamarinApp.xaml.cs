using System.Threading;
using Xamarin.Forms;
using Microsoft.AppCenter;
using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;
using System;
using System.IO;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using OmniCore.Client.Views.Base;
using OmniCore.Client.Views.Main;
using OmniCore.Model.Interfaces.Platform;
using OmniCore.Model.Interfaces.Services;
using Unity;

namespace OmniCore.Client
{
    public partial class XamarinApp : Application
    {
        public SynchronizationContext SynchronizationContext { get; }
        public Task ShutDown()
        {
            throw new NotImplementedException();
        }
        
        private readonly ICoreServices CoreServices;
        private ICoreApplicationLogger ApplicationLogger => CoreServices.ApplicationServices.ApplicationLogger;
            
        public XamarinApp(ICoreServices coreServices)
        {
            CoreServices = coreServices;

            InitializeComponent();

            MainPage = coreServices.ApplicationServices.CreateView<ShellView>();
            ApplicationLogger.Information("OmniCore App initialized");
        }
    }
}
