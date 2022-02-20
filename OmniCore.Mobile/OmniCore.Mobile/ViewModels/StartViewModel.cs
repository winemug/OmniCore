using System;
using System.Linq;
using System.Threading.Tasks;
using OmniCore.Services;
using OmniCore.Services.Entities;
using OmniCore.Services.Interfaces;
using Unity;
using Xamarin.Forms;

namespace OmniCore.Mobile.ViewModels
{
    public class StartViewModel : BaseViewModel
    {
        public StartViewModel(Page page) : base(page)
        {
            
        }
        protected override async Task InitializeAsync()
        {
            var pi = App.Container.Resolve<IPlatformInfo>();
            if (!pi.HasAllPermissions ||
                !pi.IsExemptFromBatteryOptimizations)
            {
                await Shell.Current.GoToAsync("//PlatformConfigurationPage");
            }
            else
                await Shell.Current.GoToAsync($"//LoginPage");
            
            // var apiClient = DependencyService.Get<ApiClient>();
            // var ds = DependencyService.Get<DataStore>();
            // var cs = DependencyService.Get<ConfigurationStore>();
            // var cc = await cs.GetConfigurationAsync();
            // if (cc.AccountId == null)
            // {
            //     await apiClient.AuthorizeAccount(
            //         "", "");
            //     cc.Name = "Test Client";
            //     cc = await apiClient.RegisterClient(cc);
            //     await cs.SetConfigurationAsync(cc);
            // }
            //
            // await apiClient.AuthorizeClient(cc);
            //
            // var pe = await apiClient.GetDefaultProfile();
            //
            // var epr = await apiClient.GetClientEndpoint(cc);
            // var sc = DependencyService.Get<SyncClient>();
            // await sc.StartAsync(epr);
            // await ds.EnqueueReadingsAsync();
            //
            // var xdwsc = DependencyService.Get<XDripWebServiceClient>();
            // await xdwsc.StartCollectionAsync(pe.Id);
            // await Shell.Current.GoToAsync("//StartPage");
        }
    }
}