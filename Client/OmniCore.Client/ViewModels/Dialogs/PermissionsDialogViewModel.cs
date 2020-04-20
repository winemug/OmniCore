using System;
using OmniCore.Model.Interfaces.Client;
using OmniCore.Model.Utilities.Extensions;
using Xamarin.Forms;

namespace OmniCore.Client.ViewModels.Base.Dialogs
{
    public class PermissionsDialogViewModel : DialogViewModel
    {
        public bool BluetoothPermissionGranted { get; set; }
        public bool StoragePermissionsGranted { get; set; }

        public PermissionsDialogViewModel(
            IClient client,
            IClientFunctions clientFunctions) : base(client)
        {
            ConfirmCommand = new Command(async () =>
            {
                if (!BluetoothPermissionGranted)
                    BluetoothPermissionGranted = await clientFunctions.RequestBluetoothPermission(); 
               
                if (!StoragePermissionsGranted)
                    StoragePermissionsGranted = await clientFunctions.RequestStoragePermission();

                if (BluetoothPermissionGranted && StoragePermissionsGranted)
                    await ConfirmAction();
            });

            WhenPageAppears().Subscribe(async _ =>
            {
                BluetoothPermissionGranted = await clientFunctions.BluetoothPermissionGranted();
                StoragePermissionsGranted = await clientFunctions.StoragePermissionGranted();
            }).AutoDispose(this);
        }
    }
}