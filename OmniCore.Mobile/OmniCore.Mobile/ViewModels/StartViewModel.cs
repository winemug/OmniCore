using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OmniCore.Mobile.Views;
using OmniCore.Services;
using OmniCore.Services.Entities;
using OmniCore.Services.Interfaces;
using Unity;
using Xamarin.Forms;
using Xamarin.Forms.Internals;

namespace OmniCore.Mobile.ViewModels
{
    public class StartViewModel : BaseViewModel
    {
        protected override async Task OnPageShownAsync()
        {
            var platformInfo = UnityContainer.Resolve<IPlatformInfo>();
            if (!platformInfo.HasAllPermissions ||
                !platformInfo.IsExemptFromBatteryOptimizations)
            {
                await NavigationService.NavigateAsync<PlatformConfigurationPage>();
                return;
            }

            // await NavigationService.NavigateAsync<AmqpTestPage>();
            
            await NavigationService.NavigateAsync<BluetoothTestPage>();

            // var configurationStore = UnityContainer.Resolve<ConfigurationStore>();
            // var apiClient = UnityContainer.Resolve<ApiClient>();
            // var syncClient = UnityContainer.Resolve<SyncClient>();
            // var bgcService = UnityContainer.Resolve<BgcService>();
            
            // var cc = await configurationStore.GetConfigurationAsync();
            // if (!cc.AccountId.HasValue || !cc.ClientId.HasValue || cc.ClientAuthorizationToken == null)
            // {
            //     await NavigationService.NavigateAsync<AccountLoginPage>();
            //     return;
            // }
            //
            // await apiClient.AuthorizeClientAsync(cc);
            //
            // if (!cc.DefaultProfileId.HasValue)
            // {
            //     var profile = await apiClient.GetDefaultProfileAsync();
            //     cc.DefaultProfileId = profile.Id;
            //     await configurationStore.SetConfigurationAsync(cc);
            // }
            //
            // var epr = await apiClient.GetClientEndpointAsync(cc);
            // await syncClient.StartAsync(epr);
            //
            // await bgcService.InitializeAsync();
            // if (cc.ReceiverType.HasValue)
            // {
            //     switch (cc.ReceiverType)
            //     {
            //         case CgmReceiverType.XdripWebService:
            //             var xdws = UnityContainer.Resolve<XDripWebServiceClient>();
            //             await xdws.StartCollectionAsync();
            //             break;
            //         case CgmReceiverType.NightscoutWebService:
            //             break;
            //         case CgmReceiverType.DexcomWebService:
            //             break;
            //         default:
            //             throw new ApplicationException("Unsupported cgm type");
            //     }
            // }
        }

    }
}