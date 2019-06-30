using OmniCore.Model.Data;
using OmniCore.Model.Eros;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace OmniCore.Mobile.ViewModels.Settings
{
    public class ApplicationSettingsViewModel : BaseViewModel
    {
        private bool acceptAAPSCommands;
        public bool AcceptAAPSCommands { get => acceptAAPSCommands; set => SetProperty(ref acceptAAPSCommands, value); }

        private OmniCoreSettings Settings;

        public ApplicationSettingsViewModel(Page page): base(page)
        {
        }

        protected async override Task OnAppearing()
        {
        }

        protected async override Task OnDisappearing()
        {
            Settings.AcceptCommandsFromAAPS = this.AcceptAAPSCommands;
            ErosRepository.Instance.SaveOmniCoreSettings(Settings);
        }

        protected async override Task<object> BindData()
        {
            Settings = ErosRepository.Instance.GetOmniCoreSettings();
            this.AcceptAAPSCommands = Settings.AcceptCommandsFromAAPS;
            return this;
        }

        protected override void OnDisposeManagedResources()
        {
        }
    }
}
