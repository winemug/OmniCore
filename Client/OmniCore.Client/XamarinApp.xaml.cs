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
using OmniCore.Model.Interfaces.Services;

namespace OmniCore.Client
{
    public partial class XamarinApp : Application
    {
        public XamarinApp(ICoreServices services)
        {
            InitializeComponent();

            MainPage = services.Container.Get<ShellView>();
            services.LoggingService.Information("OmniCore App initialized");
        }
    }
}
