using System.Windows.Input;
using OmniCore.Common.Platform;

namespace OmniCore.Maui.ViewModels;

public class SetupPermissionsModel
{
    private IPlatformService _platformService;
    private IPlatformInfo _platformInfo;
    public ICommand VerifyPermissionsCommand { get; set; }
    
    public string StatusText { get; set; }
    public SetupPermissionsModel(IPlatformService platformService, IPlatformInfo platformInfo)
    {
        _platformService = platformService;
        _platformInfo = platformInfo;
        VerifyPermissionsCommand = new Command(async () => await ExecuteVerifyPermissions());
    }

    private async Task ExecuteVerifyPermissions()
    {
        if (await _platformInfo.VerifyPermissions(false))
        {
            StatusText = "Good";
        }
        else
        {
            StatusText = "Not good";
        }
    }
}