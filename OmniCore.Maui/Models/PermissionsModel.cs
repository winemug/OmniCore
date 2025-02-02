using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OmniCore.Services.Interfaces.Platform;

namespace OmniCore.Maui.Models;

public partial class PermissionsModel(IPlatformService platformService) : PageModel
{
    [ObservableProperty] List<DynamicPermission> _dynamicPermissions =
    [
        new DynamicPermission(permissionName: "android.permission.INTERNET", true),
        new DynamicPermission(permissionName: "android.permission.ACCESS_NETWORK_STATE", true),
        
        new DynamicPermission(permissionName: "android.permission.BLUETOOTH_CONNECT", true),
        new DynamicPermission(permissionName: "android.permission.BLUETOOTH_SCAN", true),
        
        new DynamicPermission(permissionName: "android.permission.POST_NOTIFICATIONS", true),
        new DynamicPermission(permissionName: "android.permission.FOREGROUND_SERVICE", true),
        new DynamicPermission(permissionName: "android.permission.FOREGROUND_SERVICE_SYSTEM_EXEMPTED", true),
        new DynamicPermission(permissionName: "android.permission.USE_EXACT_ALARM", true),
    ];

    protected override async Task Appearing()
    {
        foreach (var permission in DynamicPermissions)
        {
            await permission.UpdateStatusAsync();
        }
    }

    [RelayCommand]
    public async Task RequestAllPermissions()
    {
        foreach (var permission in DynamicPermissions)
        {
            await permission.RequestAndUpdateStatusAsync();
        }
    }
}
public class DynamicPermission : Permissions.BasePlatformPermission, INotifyPropertyChanged
{
    private readonly string _permissionName;
    private readonly bool _isRuntime;

    public DynamicPermission(string permissionName, bool isRuntime)
    {
        _permissionName = permissionName;
        _isRuntime = isRuntime;
        RequestAndUpdateCommand = new RelayCommand(async () =>
        {
            await RequestAndUpdateStatusAsync();
        });
    }
    private PermissionStatus _lastStatus = PermissionStatus.Unknown;
    public string ShortName => _permissionName.Substring(_permissionName.LastIndexOf('.') + 1);
    public bool IsGranted => _lastStatus == PermissionStatus.Granted;
    public ICommand RequestAndUpdateCommand { get; }
    public override (string androidPermission, bool isRuntime)[] RequiredPermissions => new[]
    {
        (_permissionName, _isRuntime),
    };

    public async Task UpdateStatusAsync()
    {
        _lastStatus = await CheckStatusAsync();
        OnPropertyChanged(nameof(IsGranted));
    }

    public async Task RequestAndUpdateStatusAsync()
    {
        _lastStatus = await RequestAsync();
        OnPropertyChanged(nameof(IsGranted));
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
