using System.Windows.Input;
using OmniCore.Maui.Views;
using OmniCore.Services.Interfaces.Core;
using OmniCore.Services.Interfaces.Platform;

namespace OmniCore.Maui.ViewModels;

public class DefaultViewModel : BaseViewModel
{
    private IPlatformInfo _platformInfo;
    private IConfigurationStore _configurationStore;

    public ICommand CheckPermissionsCommand { get; set; }
    
    public DefaultViewModel(IPlatformInfo platformInfo, IConfigurationStore configurationStore)
    {
        _platformInfo = platformInfo;
        _configurationStore = configurationStore;
        CheckPermissionsCommand = new Command(CheckPermissions);
    }
    private async void CheckPermissions()
    {
        var permissionsOK = await _platformInfo.VerifyPermissions();
        if (permissionsOK)
        {
            await Shell.Current.GoToAsync("//BlankView");
        }
    }

    public override async ValueTask OnAppearing()
    {
        var cc = await _configurationStore.GetConfigurationAsync();
        if (!cc.AccountId.HasValue)
        {
            await Shell.Current.GoToAsync("//LoginAccountView");
        }
        
        if (!cc.ClientId.HasValue)
        {
            await Shell.Current.GoToAsync("//RegisterClientView");
        }

        await Shell.Current.GoToAsync("//HomeView");
    }
}