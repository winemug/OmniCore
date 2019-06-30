using OmniCore.Model.Data;
using OmniCore.Model.Eros;
using System;
using System.Collections.Generic;
using System.Text;

namespace OmniCore.Mobile.ViewModels.Settings
{
    public class ApplicationSettingsViewModel
    {
        public bool AcceptAAPSCommands
        {
            get
            {
                return Settings.AcceptCommandsFromAAPS;
            }
            set
            {
                if (value != Settings.AcceptCommandsFromAAPS)
                {
                    Settings.AcceptCommandsFromAAPS = value;
                    ErosRepository.Instance.SaveOmniCoreSettings(Settings);
                }
            }
        }

        private OmniCoreSettings Settings;
        public ApplicationSettingsViewModel()
        {
            Settings = ErosRepository.Instance.GetOmniCoreSettings();
        }
    }
}
