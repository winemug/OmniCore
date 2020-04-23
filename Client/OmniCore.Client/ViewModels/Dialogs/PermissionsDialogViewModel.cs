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
            IActivityContext activityContext) : base(client)
        {
            DialogOkCommand = new Command(async () =>
            {
                if (!BluetoothPermissionGranted)
                    BluetoothPermissionGranted = await activityContext.RequestBluetoothPermission(); 
               
                if (!StoragePermissionsGranted)
                    StoragePermissionsGranted = await activityContext.RequestStoragePermission();

                if (BluetoothPermissionGranted && StoragePermissionsGranted)
                    await ConfirmAction();
            });

            WhenPageAppears().Subscribe(async _ =>
            {
                BluetoothPermissionGranted = await activityContext.BluetoothPermissionGranted();
                StoragePermissionsGranted = await activityContext.StoragePermissionGranted();
            }).AutoDispose(this);
        }
    }
}