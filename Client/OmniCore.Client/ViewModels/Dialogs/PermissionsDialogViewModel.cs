using System;
using OmniCore.Model.Interfaces;
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
            IUserActivity userActivity) : base(client)
        {
            DialogOkCommand = new Command(async () =>
            {
                if (!BluetoothPermissionGranted)
                    BluetoothPermissionGranted = await userActivity.RequestBluetoothPermission(); 
               
                if (!StoragePermissionsGranted)
                    StoragePermissionsGranted = await userActivity.RequestStoragePermission();

                if (BluetoothPermissionGranted && StoragePermissionsGranted)
                    await ConfirmAction();
            });

            WhenPageAppears().Subscribe(async _ =>
            {
                BluetoothPermissionGranted = await userActivity.BluetoothPermissionGranted();
                StoragePermissionsGranted = await userActivity.StoragePermissionGranted();
            }).AutoDispose(this);
        }
    }
}