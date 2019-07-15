using OmniCore.Mobile.Base;
using OmniCore.Mobile.ViewModels.Settings;
using OmniCore.Model.Eros;
using Plugin.Permissions;
using Plugin.Permissions.Abstractions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace OmniCore.Mobile.Views.Settings
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    [Fody.ConfigureAwait(true)]
    public partial class GeneralSettingsPage : ContentPage
    {
        public GeneralSettingsPage()
        {
            InitializeComponent();
            new ApplicationSettingsViewModel(this);
        }

        private async void Backup_Clicked(object sender, EventArgs e)
        {
            if (!await CheckPermissions())
                return;
            var backupPath = Path.Combine(OmniCoreServices.Application.GetPublicDataPath(), "OmniCore_backup.db3");
            if (File.Exists(backupPath))
            {
                var accepted = await DisplayAlert("Database backup",
                        @"This will overwrite the existing backup on your internal storage, are you sure you want to continue?",
                        "Backup", "Cancel");

                if (!accepted)
                    return;
            }
            var repo = await ErosRepository.GetInstance();
            File.Copy(repo.DbPath, backupPath, true);
            await DisplayAlert("Database backup", "Backup completed", "OK");
        }

        private async void Restore_Clicked(object sender, EventArgs e)
        {
            if (!await CheckPermissions())
                return;
            var backupPath = Path.Combine(OmniCoreServices.Application.GetPublicDataPath(), "OmniCore_backup.db3");
            if (!File.Exists(backupPath))
            {
                await DisplayAlert("Database restore", "Backup file could not be found", "OK");
            }
            else
            {
                var accepted = await DisplayAlert("Database restore",
                        @"Restoring from this backup will overwrite your existing configuration and you will lose all information that is stored with this app. Are you sure?",
                        "Restore", "Cancel");

                if (accepted)
                {
                    var repo = await ErosRepository.GetInstance();
                    File.Copy(backupPath, repo.DbPath, true);
                    await DisplayAlert("Database restore", "Restore completed", "OK");
                }
            }
        }

        private async void Erase_Clicked(object sender, EventArgs e)
        {
            if (!await DisplayAlert("Erase Database", "WARNING: You will lose all data stored in OmniCore.", "Erase ALL", "Cancel"))
                return;

            var repo = await ErosRepository.GetInstance();
            File.Delete(repo.DbPath);
            OmniCoreServices.Application.Exit();
        }

        private async Task<bool> CheckPermissions()
        {
            var status = await CrossPermissions.Current.CheckPermissionStatusAsync(Permission.Storage);
            if (status != PermissionStatus.Granted)
            {
                await DisplayAlert("Missing Permissions", "You have to grant the storage permission to this application in order to be able to restore and backup the database.", "OK");
                var request = await CrossPermissions.Current.RequestPermissionsAsync(Permission.Storage);
                if (request[Permission.Storage] != PermissionStatus.Granted)
                {
                    await DisplayAlert("Missing Permissions", "This operation cannot be done without the necessary permissions.", "OK");
                    return false;
                }
            }

            var storagePath = OmniCoreServices.Application.GetPublicDataPath();
            if (!Directory.Exists(storagePath))
            {
                Directory.CreateDirectory(storagePath);
            }
            return true;
        }
    }
}