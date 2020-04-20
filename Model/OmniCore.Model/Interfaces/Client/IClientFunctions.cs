using System;
using System.Threading;
using System.Threading.Tasks;
using OmniCore.Model.Interfaces.Common;
using OmniCore.Model.Interfaces.Services;
using OmniCore.Model.Interfaces.Services.Internal;

namespace OmniCore.Model.Interfaces.Client
{
    public interface IClientFunctions : IClientInstance
    {
        Task AttachToService(Type concreteType, IClientConnection connection);
        Task DetachFromService(IClientConnection connection);
        IObservable<(string Permission, bool IsGranted)> RequestPermissions(params string[] permissions);
        Task<bool> PermissionGranted(string permission);
        
        Task<bool> BluetoothPermissionGranted();
        Task<bool> StoragePermissionGranted();

        Task<bool> RequestBluetoothPermission();
        Task<bool> RequestStoragePermission();
        IForegroundTask CreateForegroundTask();
    }
}