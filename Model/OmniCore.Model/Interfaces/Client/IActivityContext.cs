using System;
using System.Threading;
using System.Threading.Tasks;
using OmniCore.Model.Interfaces.Services;

namespace OmniCore.Model.Interfaces.Client
{
    public interface IActivityContext
    {
        Task<IForegroundTaskService> GetForegroundTaskService(CancellationToken cancellationToken);
        Task AttachToService(Type concreteType, IClientConnection connection);
        Task DetachFromService(IClientConnection connection);
        IObservable<(string Permission, bool IsGranted)> RequestPermissions(params string[] permissions);
        Task<bool> PermissionGranted(string permission);
        
        Task<bool> BluetoothPermissionGranted();
        Task<bool> StoragePermissionGranted();

        Task<bool> RequestBluetoothPermission();
        Task<bool> RequestStoragePermission();
    }}