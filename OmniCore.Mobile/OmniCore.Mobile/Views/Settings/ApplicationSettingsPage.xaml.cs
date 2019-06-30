using OmniCore.Mobile.Base;
using OmniCore.Mobile.ViewModels.Settings;
using OmniCore.Model.Eros;
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
            BindingContext = new ApplicationSettingsViewModel();
        }

        private async void Backup_Clicked(object sender, EventArgs e)
        {
            var backupPath = Path.Combine(OmniCoreServices.Application.GetPublicDataPath(), "OmniCore_backup.db3");
            if (File.Exists(backupPath))
            {
                var accepted = await DisplayAlert("Database backup",
                        @"This will overwrite the existing backup on your internal storage, are you sure you want to continue?",
                        "Backup", "Cancel");

                if (!accepted)
                    return;
            }
            File.Copy(ErosRepository.Instance.DbPath, backupPath, true);
            await DisplayAlert("Database backup", "Backup completed", "OK");
        }

        private async void Restore_Clicked(object sender, EventArgs e)
        {
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
                    File.Copy(backupPath, ErosRepository.Instance.DbPath, true);
                    await DisplayAlert("Database restore", "Restore completed", "OK");
                }
            }
        }
    }
}