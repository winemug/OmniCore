using System.Windows.Input;
using OmniCore.Common.Platform;

namespace OmniCore.Maui.ViewModels;

public class SetupPermissionsModel
{
    private readonly IPlatformInfo _platformInfo;
    private IPlatformService _platformService;

    public SetupPermissionsModel(IPlatformService platformService, IPlatformInfo platformInfo)
    {
        _platformService = platformService;
        _platformInfo = platformInfo;
        VerifyPermissionsCommand = new Command(async () => await ExecuteVerifyPermissions());
    }

    public ICommand VerifyPermissionsCommand { get; set; }

    public string StatusText { get; set; }

    private async Task ExecuteVerifyPermissions()
    {
        if (await _platformInfo.VerifyPermissions(false))
            StatusText = "Good";
        else
            StatusText = "Not good";
    }
}