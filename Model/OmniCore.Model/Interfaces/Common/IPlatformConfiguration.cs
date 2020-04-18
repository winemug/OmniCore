using System;
using System.Threading.Tasks;
using OmniCore.Model.Interfaces.Client;
using OmniCore.Model.Interfaces.Services;

namespace OmniCore.Model.Interfaces.Common
{
    public interface IPlatformConfiguration : IClientInstance, IServiceInstance
    {
        bool ServiceEnabled { get; set; }
        bool TermsAccepted { get; set; }

        Task<bool> BluetoothPermissionGranted();
        Task<bool> StoragePermissionGranted();

        Task<bool> RequestBluetoothPermission();
        Task<bool> RequestStoragePermission();
    }
}