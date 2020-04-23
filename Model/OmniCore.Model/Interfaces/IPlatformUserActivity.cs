using System;
using System.Threading;
using System.Threading.Tasks;
using OmniCore.Model.Interfaces.Services;

namespace OmniCore.Model.Interfaces
{
    public interface IPlatformUserActivity
    {
        Task<IForegroundTaskService> GetForegroundTaskService(CancellationToken cancellationToken);
        IObservable<(string Permission, bool IsGranted)> RequestPermissions(params string[] permissions);
        Task<bool> PermissionGranted(string permission);
        
        Task<bool> BluetoothPermissionGranted();
        Task<bool> StoragePermissionGranted();

        Task<bool> RequestBluetoothPermission();
        Task<bool> RequestStoragePermission();
    }}