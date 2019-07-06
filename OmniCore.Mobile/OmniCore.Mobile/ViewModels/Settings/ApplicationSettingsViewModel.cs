using OmniCore.Model.Data;
using OmniCore.Model.Eros;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace OmniCore.Mobile.ViewModels.Settings
{
    public class ApplicationSettingsViewModel : PageViewModel
    {
        public bool AcceptAAPSCommands { get; set; }

        private OmniCoreSettings Settings;

        public ApplicationSettingsViewModel(Page page): base(page)
        {
        }

        [method: SuppressMessage("", "CS1998", Justification = "Not applicable")]
        protected async override Task OnAppearing()
        {
        }

        [method: SuppressMessage("", "CS1998", Justification = "Not applicable")]
        protected async override Task OnDisappearing()
        {
            Settings.AcceptCommandsFromAAPS = this.AcceptAAPSCommands;
            ErosRepository.Instance.SaveOmniCoreSettings(Settings);
        }

        [method: SuppressMessage("", "CS1998", Justification = "Not applicable")]
        protected async override Task<BaseViewModel> BindData()
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
