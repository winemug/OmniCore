using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;

namespace OmniCore.Maui.Models;

public class DynamicPermissionModel : Permissions.BasePlatformPermission, INotifyPropertyChanged
{
    private readonly string _permissionName;
    private readonly bool _isRuntime;

    public DynamicPermissionModel(string permissionName, bool isRuntime)
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