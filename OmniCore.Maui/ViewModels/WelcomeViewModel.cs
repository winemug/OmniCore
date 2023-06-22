using System.Windows.Input;
using OmniCore.Services.Interfaces.Platform;

namespace OmniCore.Maui.ViewModels;

public class WelcomeViewModel : BaseViewModel
{
    private IPlatformService _platformService;
    private IPlatformInfo _platformInfo;
    public ICommand VerifyPermissionsCommand { get; set; }
    public WelcomeViewModel(IPlatformService platformService, IPlatformInfo platformInfo)
    {
        _platformService = platformService;
        _platformInfo = platformInfo;
        VerifyPermissionsCommand = new Command(async () => await ExecuteVerifyPermissions());
    }

    private async Task ExecuteVerifyPermissions()
    {
        if (await _platformInfo.VerifyPermissions())
        {
        }
    }
}