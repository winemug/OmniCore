using System;
using System.Reactive.Threading.Tasks;
using System.Threading.Tasks;
using System.Windows.Input;
using OmniCore.Client.ViewModels.Base;
using OmniCore.Client.Views.Wizards.NewUser;
using OmniCore.Model.Interfaces.Client;
using OmniCore.Model.Interfaces.Common;
using OmniCore.Model.Utilities.Extensions;
using Xamarin.Forms;

namespace OmniCore.Client.ViewModels.Wizards
{
    public class PermissionsWizardViewModel : BaseViewModel
    {
        public bool BluetoothPermissionGranted { get; set; }
        public bool StoragePermissionsGranted { get; set; }
        
        public ICommand ContinueCommand { get; }
        public ICommand ExitCommand { get; }

        private readonly IPlatformConfiguration PlatformConfiguration;
        public PermissionsWizardViewModel(IClient client,
            ICommonFunctions commonFunctions,
            IPlatformConfiguration platformConfiguration,
            IClientFunctions clientFunctions) : base(client)
        {
            PlatformConfiguration = platformConfiguration;
            ContinueCommand = new Command(async () =>
            {
                if (!BluetoothPermissionGranted)
                    BluetoothPermissionGranted = await platformConfiguration.RequestBluetoothPermission(); 
               
                if (!StoragePermissionsGranted)
                    StoragePermissionsGranted = await platformConfiguration.RequestStoragePermission();
                
                if (BluetoothPermissionGranted && StoragePermissionsGranted)
                    await client.PushView<UserWizardRootView>();
            });

            ExitCommand = new Command(commonFunctions.Exit);
            
            WhenPageAppears().Subscribe(async _ =>
            {
                BluetoothPermissionGranted = await platformConfiguration.BluetoothPermissionGranted();
                StoragePermissionsGranted = await platformConfiguration.StoragePermissionGranted();
            }).AutoDispose(this);
        }
    }
}